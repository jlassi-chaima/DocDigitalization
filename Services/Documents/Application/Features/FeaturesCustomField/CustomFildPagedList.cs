

using Application.Dtos.CustomField;
using Application.Parameters;
using Application.Respository;
using DD.Core.Pagination;
using Domain.DocumentManagement.CustomFields;
using MediatR;

namespace Application.Features.FeaturesCustomField
{
    public class CustomFildPagedList
    {
        public sealed record Query : IRequest<PagedList<CustomFieldDetails>>
        {
            public CustomFieldParameters customfiledparameters { get; set; }
            public Query(CustomFieldParameters customparam)
            {
                customfiledparameters = customparam;

            }
        }
        public sealed class Handler : IRequestHandler<Query, PagedList<CustomFieldDetails>>
        {
            private readonly ICustomFieldRepository _repository;

            public Handler(ICustomFieldRepository repository)
            {
                _repository = repository;
            }

            public async Task<PagedList<CustomFieldDetails>> Handle(Query request, CancellationToken cancellationToken)
            {
                return await _repository.GetPagedCustomFieldAsync<CustomFieldDetails>(request.customfiledparameters);

            }
        }
    }
}
