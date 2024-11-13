using Application.Exceptions;
using Application.Respository;
using Domain.DocumentManagement.CustomFields;
using MediatR;
using Serilog;

namespace Application.Features.FeaturesCustomField
{
    public class DetailsCustomField
    {
        public sealed record Query : IRequest<CustomField>
        {
            public readonly Guid Id;
            public Query(Guid id)
            {
                Id = id;
            }
        }
        public sealed class Handler : IRequestHandler<Query, CustomField>
        {
            private readonly ICustomFieldRepository _customFieldRepository;

            public Handler(ICustomFieldRepository customFieldRepository)
            {
                _customFieldRepository = customFieldRepository;
            }

            public async Task<CustomField> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {

                    CustomField customFieldDetails = await _customFieldRepository.FindByIdAsync(request.Id, cancellationToken);
                    if (customFieldDetails == null)
                    {
                        throw new CustomFieldException($"Custom Field with ID {request.Id} not found.");
                    }
                    return customFieldDetails;
                }
                catch (CustomFieldException ex)
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
