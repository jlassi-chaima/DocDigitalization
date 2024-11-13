using Application.Dtos.Templates;
using Application.Respository;
using Application.RestApiMail.Dto;
using Application.RestApiMail.EndPoints;
using Domain.Templates;
using Domain.Templates.Enum;
using FluentValidation;
using MapsterMapper;
using MediatR;


namespace Application.Features.FeaturesTemplates
{
    public class AddTemplate
    {
        public sealed record Command : IRequest<Template>
        {
            public readonly TemplateDto template;

            public Command(TemplateDto templatedto)
            {
                template = templatedto;
            }
        }

        public sealed class AddValidator : AbstractValidator<Command>
        {
            public AddValidator(ITemplateRepository _templateRepository)
            {
                RuleFor(mr => mr.template.Name).NotEmpty().MustAsync(async (name, ct) => !await _templateRepository.ExistsAsync(mr => mr.Name == name, ct))
                                 .WithMessage("Name must be unique.");

                RuleFor(mr => mr.template.Order).NotEmpty().MustAsync(async (order, ct) => !await _templateRepository.ExistsAsync(mr => mr.Order == order, ct))
                                 .WithMessage("Order must be unique.");

            }
        }

        public sealed class Handler : IRequestHandler<Command, Template>
        {
            private readonly ITemplateRepository _templateRepository;
            private readonly IDocumentRepository _documentRepository;
            private readonly Mapper _mapper;

            public Handler(Mapper mapper, ITemplateRepository templateRepository)
            {
                _mapper = mapper;
                _templateRepository = templateRepository;
            }
            public async Task<Template> Handle(Command request, CancellationToken cancellationToken)
            {
                Template templatetoadd = new Template();

                if (request.template.Type == GetListByType.Started)
                {
                    // Create a new template instance
                    templatetoadd = new Template
                    {
                        Name = request.template.Name,
                        Order = request.template.Order,
                        Sources = request.template.Sources,
                        FilterFilename = request.template.Filter_filename,
                        FilterPath = request.template.Filter_path,
                        FilterMailrule = request.template.Filter_mailrule,
                        AssignTitle = request.template.Assign_title,
                        AssignTags = request.template.Assign_tags,
                        AssignDocumentType = request.template.Assign_document_type,
                        AssignCorrespondent = request.template.Assign_correspondent,
                        AssignStoragePath = request.template.Assign_storage_path,
              
                        Type = request.template.Type,
                        Content_matching_algorithm = null,
                        Content_matching_pattern = null,
                        Has_Tags = null,
                        Has_Correspondent = null,
                        Has_Document_Type = null,
                        Is_Insensitive = null,
                        DocumentClassification = request.template.DocumentClassification,
                        Owner = request.template.Owner,
                        Is_Enabled = request.template.Is_Enabled,
                        Assign_view_users = request.template.Assign_view_users,
                        Assign_change_groups = request.template.Assign_change_groups,
                        Assign_change_users = request.template.Assign_change_users,
                        Assign_view_groups = request.template.Assign_view_groups,
                        

                    };

                    List<MailRule> mailrules = MailRulesList.CallRestApi().Result;
                    if(mailrules != null)
                    {
                        // Check if templatetoadd.FilterMailrule is in the mailrules list
                        bool filterMailRuleExists = false;
                        foreach (var rule in mailrules)
                        {
                            if (rule.Name == templatetoadd.FilterMailrule) // Assuming MailRule has appropriate equality comparison
                            {
                                filterMailRuleExists = true;
                                break;
                            }
                        }

                        // If templatetoadd.FilterMailrule does not exist, set it to null
                        if (!filterMailRuleExists)
                        {
                            templatetoadd.FilterMailrule = null;
                        }
                    }
                 

                }
                else if (request.template.Type == GetListByType.Updated || request.template.Type == GetListByType.Added)
                {
                    templatetoadd = new Template
                    {
                        Name = request.template.Name,
                        Order = request.template.Order,
                        Sources = null,
                        FilterFilename = request.template.Filter_filename,
                        FilterPath = null,
                        FilterMailrule = null,
                        AssignTitle = request.template.Assign_title,
                        AssignTags = request.template.Assign_tags,
                        AssignDocumentType = request.template.Assign_document_type,
                        AssignCorrespondent = request.template.Assign_correspondent,
                        AssignStoragePath = request.template.Assign_storage_path,
                        Type = request.template.Type,
                        Content_matching_algorithm = request.template.Matching_algorithm,
                        Content_matching_pattern = request.template.match,
                        Has_Tags = request.template.Filter_has_tags,
                        Has_Correspondent = request.template.Filter_has_correspondent,
                        Has_Document_Type = request.template.Filter_has_document_type,
                        Is_Insensitive = request.template.Is_insensitive,
                        Owner = request.template.Owner,
                        Is_Enabled = request.template.Is_Enabled,
                        Assign_view_users = request.template.Assign_view_users,
                        Assign_change_groups = request.template.Assign_change_groups,
                        Assign_change_users = request.template.Assign_change_users,
                        Assign_view_groups = request.template.Assign_view_groups,
                    };
                }


                // Add the template to the database
                await _templateRepository.AddAsync(templatetoadd, cancellationToken);

                return templatetoadd;
            }




        }
    }
}
