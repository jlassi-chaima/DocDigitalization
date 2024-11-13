using Application.Dtos.Templates;
using Application.Parameters;
using Application.Respository;
using DD.Core.Pagination;
using MediatR;


namespace Application.Features.FeaturesTemplates
{
    public class ListTemplate
    {
        public sealed record Query : IRequest<PagedList<PagedTemplate>>
        {
            public TemplateParametres templateparameters { get; set; }
            public readonly string? Owner;
          
            public Query(TemplateParametres temparam, string owner)
            {
                templateparameters = temparam;
                Owner = owner;
            }
        }
        public sealed class Handler : IRequestHandler<Query, PagedList<PagedTemplate>>
        {
            private readonly ITemplateRepository _repository;

            public Handler(ITemplateRepository repository)
            {
                _repository = repository;
            }

            public async Task<PagedList<PagedTemplate>> Handle(Query request, CancellationToken cancellationToken)
            {
                return await _repository.GetPagedTemplatesAsync(request.templateparameters, request.Owner);
            }
        }
    }
}
