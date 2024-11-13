using Application.Dtos.Templates;
using Application.Respository;
using Domain.Logs;
using Domain.Templates;
using MapsterMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.FeaturesTemplates
{
    public class UpdateTemplate
    {
        public sealed record Command : IRequest<Template>
        {
            public readonly Guid TemplateId;
            public readonly TemplateDto TemplateToUpdate;

            public Command(TemplateDto templatetoupdate, Guid id)
            {
                TemplateToUpdate = templatetoupdate;
                TemplateId = id;
            }
        }

        public sealed class Handler : IRequestHandler<Command, Template>
        {
            private readonly ITemplateRepository _repository;
            private readonly IMapper _mapper;
            private readonly ILogRepository _logRepository;
            public Handler(ITemplateRepository repository, IMapper mapper, ILogRepository logRepository)
            {
                _repository = repository;
                _mapper = mapper;
                _logRepository = logRepository;
            }

            //public async Task<Template> Handle(Command request, CancellationToken cancellationToken)
            //{
            //    Logs new_note = Logs.Create(LogLevel.INFO, LogName.DigitalWork, $"Template updated {request.TemplateToUpdate.Name}");
            //    await _logRepository.AddAsync(new_note);

            //    Template templatetoupdate = _repository.FindByIdAsync(request.TemplateId, cancellationToken).GetAwaiter().GetResult();

            //    _mapper.Map(request.TemplateToUpdate, templatetoupdate);
            //    await _repository.UpdateAsync(templatetoupdate);
            //    return templatetoupdate;

            //}
            public async Task<Template> Handle(Command request, CancellationToken cancellationToken)
            {
                Logs new_note = Logs.Create(LogLevel.INFO, LogName.EasyDoc, $"Template updated {request.TemplateToUpdate.Name}");
                await _logRepository.AddAsync(new_note);

                Template templateToUpdate = await _repository.FindByIdAsync(request.TemplateId, cancellationToken);

                if (templateToUpdate == null)
                {
                    throw new KeyNotFoundException($"Template with ID {request.TemplateId} not found.");
                }

             
                    // Fallback to manual mapping if the mapper fails
                    templateToUpdate.Name = request.TemplateToUpdate.Name;
                    templateToUpdate.Order = request.TemplateToUpdate.Order;
                    templateToUpdate.DocumentClassification = request.TemplateToUpdate.DocumentClassification;
                    templateToUpdate.Sources = request.TemplateToUpdate.Sources;
                    templateToUpdate.FilterFilename = request.TemplateToUpdate.Filter_filename;
                    templateToUpdate.FilterPath = request.TemplateToUpdate.Filter_path;
                    templateToUpdate.FilterMailrule = request.TemplateToUpdate.Filter_mailrule;
                    templateToUpdate.AssignTitle = request.TemplateToUpdate.Assign_title;
                    templateToUpdate.AssignTags = request.TemplateToUpdate.Assign_tags;
                    templateToUpdate.AssignDocumentType = request.TemplateToUpdate.Assign_document_type;
                    templateToUpdate.AssignCorrespondent = request.TemplateToUpdate.Assign_correspondent;
                    templateToUpdate.AssignStoragePath = request.TemplateToUpdate.Assign_storage_path;
                    templateToUpdate.Type = request.TemplateToUpdate.Type;
                    templateToUpdate.Content_matching_algorithm = request.TemplateToUpdate.Matching_algorithm;
                    templateToUpdate.Content_matching_pattern = request.TemplateToUpdate.match;
                    templateToUpdate.Has_Tags = request.TemplateToUpdate.Filter_has_tags;
                    templateToUpdate.Has_Correspondent = request.TemplateToUpdate.Filter_has_correspondent;
                    templateToUpdate.Has_Document_Type = request.TemplateToUpdate.Filter_has_document_type;
                    templateToUpdate.Owner = request.TemplateToUpdate.Owner;
                    templateToUpdate.Is_Insensitive = request.TemplateToUpdate.Is_insensitive;
                    templateToUpdate.Is_Enabled = request.TemplateToUpdate.Is_Enabled;
                    templateToUpdate.Assign_view_users = request.TemplateToUpdate.Assign_view_users;
                    templateToUpdate.Assign_view_groups = request.TemplateToUpdate.Assign_view_groups;
                    templateToUpdate.Assign_change_users = request.TemplateToUpdate.Assign_change_users;
                    templateToUpdate.Assign_change_groups = request.TemplateToUpdate.Assign_change_groups;
                

                await _repository.UpdateAsync(templateToUpdate, cancellationToken);
                return templateToUpdate;
            }
        }
    }
}
