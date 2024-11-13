using Application.Exceptions;
using Application.Respository;
using Aspose.Pdf.Operators;
using Core.Exceptions;
using Domain.DocumentManagement.CustomFields;
using Domain.Logs;
using MediatR;
using Serilog;


namespace Application.Features.FeaturesCustomField
{
    public class DeleteCustomField
    {
        public sealed record Command : IRequest
        {
            public readonly Guid Id;
            public Command(Guid id)
            {
                Id = id;
            }
        }
        public sealed class Handler : IRequestHandler<Command>
        {
            private readonly ICustomFieldRepository _customFieldRepository;
            private readonly ILogRepository _logRepository;
            public Handler(ICustomFieldRepository customFieldRepository, ILogRepository logRepository)
            {
                _customFieldRepository = customFieldRepository;
                _logRepository = logRepository;
            }

            public async Task Handle(Command request, CancellationToken cancellationToken)
            {
                try
                {
                    CustomField customfieldToDelete = await _customFieldRepository.FindByIdAsync(request.Id, cancellationToken);
                    if(customfieldToDelete == null)
                    {
                        throw new NotFoundException("Custom field not Found.");

                    }
                    Logs new_custom_field = Logs.Create(LogLevel.INFO, LogName.EasyDoc, $"Custom Field deleted {customfieldToDelete.Name}");
                    await _logRepository.AddAsync(new_custom_field);
                    await _customFieldRepository.DeleteAsync(customfieldToDelete, cancellationToken);
                }
                catch (CustomFieldException ex)
                {
                    Log.Error(ex.Message);
                    throw new Exception(ex.Message);
                }
                catch (NotFoundException ex)
                {
                    Log.Error(ex.Message);
                    throw new Exception(ex.Message);
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                    throw new Exception(ex.Message);
                }
            }
        }
    }
}
