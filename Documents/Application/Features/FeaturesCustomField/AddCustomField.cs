using Application.Dtos.CustomField;
using Application.Respository;
using Application.Services;
using Domain.DocumentManagement.CustomFields;
using Domain.Logs;
using FluentValidation;
using MapsterMapper;
using MediatR;

namespace Application.Features.FeaturesCustomField
{
    public class AddCustomField
    {
        public sealed record Command : IRequest<CustomField>
        {
            public readonly CustomFieldDto customfield;

            public Command(CustomFieldDto customfieldto)
            {
                customfield = customfieldto;
            }
        }
        public sealed class Validator : AbstractValidator<Command>
        {
            public Validator(ICustomFieldRepository _repository)
            {
                RuleFor(c => c.customfield.Name).NotEmpty().WithMessage("This field may not be null.");
                RuleFor(c => c.customfield.Data_type).Must(i=>Enum.IsDefined(typeof(TypeField) ,i)).WithMessage("This field may not be null.");
            }
        }
        public sealed class Handler : IRequestHandler<Command, CustomField>
        {
            private readonly ICustomFieldRepository _repository;
            private readonly IMapper _mapper;
            private readonly ILogRepository _logRepository;
            public Handler(ICustomFieldRepository repository, IMapper mapper, ILogRepository logRepository)
            {
                _repository = repository;
                _mapper = mapper;
                _logRepository = logRepository;
            }
            public async Task<CustomField> Handle(Command request, CancellationToken cancellationToken)
            {
                Logs new_custom_field = Logs.Create(LogLevel.INFO, LogName.EasyDoc, $"new Custom Field added {request.customfield.Name}");
                await _logRepository.AddAsync(new_custom_field);

                
                var customToAdd = CustomField.Create(
                 request.customfield.Name,
                 request.customfield.Data_type
                 );
                await _repository.AddAsync(customToAdd);
                return _mapper.Map<CustomField>( customToAdd );


            }
        }
    }
}