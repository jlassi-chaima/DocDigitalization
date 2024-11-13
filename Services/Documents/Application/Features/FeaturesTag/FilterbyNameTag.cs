using Application.Dtos.Tag;
using Application.Parameters;
using Application.Respository;
using DD.Core.Pagination;
using Domain.DocumentManagement.tags;
using MediatR;

namespace Application.Features.FeaturesTag
{
    public class FilterbyNameTag
    {
        public sealed record Query : IRequest<PagedList<TagDtoDetails>>
        {
            public TagParameters tagparameters { get; set; }
            public readonly string Name;
            public readonly string Owner;
            public Query(string namefilter, TagParameters tagparam, string owner)
            {
                Name = namefilter;
                tagparameters = tagparam;
                Owner = owner;
            }
        }
        public class Handler : IRequestHandler<Query, PagedList<TagDtoDetails>>
        {
            private readonly ITagRepository _tagRepository;

            public Handler(ITagRepository tagRepository)
            {
                _tagRepository = tagRepository;
            }

            public async Task<PagedList<TagDtoDetails>> Handle(Query request, CancellationToken cancellationToken)
            {
                // Use the repository to fetch tags by name
                return await _tagRepository.GetTagsByNameAsync(request.tagparameters,request.Name,request.Owner);
            }
        }
    }
}
