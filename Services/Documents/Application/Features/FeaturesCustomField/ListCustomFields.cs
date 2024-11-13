using Application.Dtos.CustomField;
using Application.Parameters;
using Application.Respository;
using DD.Core.Pagination;
using Domain.DocumentManagement.CustomFields;
using MediatR;


namespace Application.Features.FeaturesCustomField
{
    public class ListCustomFields
    {
        public sealed record Query : IRequest<List<CustomFieldDetails>>
        {
           
        }
        public sealed class Handler : IRequestHandler<Query, List<CustomFieldDetails>>
        {
            private readonly ICustomFieldRepository _repository;

            public Handler(ICustomFieldRepository repository)
            {
                _repository = repository;
            }

            public async Task<List<CustomFieldDetails>> Handle(Query request, CancellationToken cancellationToken)
            {
                return (List<CustomFieldDetails>)await _repository.GetAllDetailsAsync();
                
            }
        }

    }
}
