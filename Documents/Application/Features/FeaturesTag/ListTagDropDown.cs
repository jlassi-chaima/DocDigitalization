using Application.Dtos.Tag;
using Application.Parameters;
using Application.Respository;
using Core.Events;
using DD.Core.Pagination;
using MediatR;

namespace Application.Features.FeaturesTag
{
    public class ListTagDropDown
    {
        public sealed record Query : IRequest<PagedList<TagDtoDetails>>
        {
            public TagParameters tagparameters { get; set; }
            public string? Owner { get; set; }
            public Query(string? owner, TagParameters tagparam)
            {
                Owner = owner;
                tagparameters = tagparam;

            }
        }
        public sealed class Handler : IRequestHandler<Query, PagedList<TagDtoDetails>>
        {
            private readonly ITagRepository _repository;
          
            public Handler(ITagRepository repository, IEventPublisher eventBus)
            {
                _repository = repository;
              
            }

            public async Task<PagedList<TagDtoDetails>> Handle(Query request, CancellationToken cancellationToken)
            {
               

                var tags = await _repository.GetPagedtagAsync<TagDtoDetails>(request.tagparameters, request.Owner,null, cancellationToken);

                return tags;

            }
        }
    }
}
