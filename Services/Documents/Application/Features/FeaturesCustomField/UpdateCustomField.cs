using Application.Dtos.CustomField;
using Application.Respository;
using Domain.DocumentManagement.CustomFields;
using Domain.Logs;
using FluentValidation;
using MapsterMapper;
using MediatR;

namespace Application.Features.FeaturesCustomField
{
    public class UpdateCustomField
    {
        public sealed record Command : IRequest<CustomField>
        {
            public readonly Guid customfieldId;
            public readonly CustomFieldDto customfieldupdate;

            public Command(CustomFieldDto customfieldtoupdate, Guid id)
            {
                customfieldupdate = customfieldtoupdate;
                customfieldId = id;
            }
        }
        //public sealed class Validator : AbstractValidator<Command>
        //{
        //    public Validator(ICustomFieldRepository _repository)
        //    {
        //        RuleFor(p => p.customfieldupdate.Data_type).Empty();
        //    }
        //}
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
                Logs new_custom_field = Logs.Create(LogLevel.INFO, LogName.EasyDoc, $"Custom Field updated {request.customfieldupdate.Name}");
                await _logRepository.AddAsync(new_custom_field);

                CustomField customtoupdate = _repository.FindByIdAsync(request.customfieldId, cancellationToken).GetAwaiter().GetResult();
                /*if(customtoupdate.DataType == request.customfieldupdate.DataType) */
                _mapper.Map(request.customfieldupdate, customtoupdate);
                await _repository.UpdateAsync(customtoupdate);
                //}

                return customtoupdate;



            }

        }

    }

}



