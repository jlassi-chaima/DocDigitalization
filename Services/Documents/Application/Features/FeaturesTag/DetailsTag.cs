using Application.Respository;
using Domain.DocumentManagement.tags;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.FeaturesTag
{
    public class DetailsTag
    {
        public sealed record Query : IRequest<Tag>
        {
            public readonly Guid Id;
            public Query(Guid id)
            {
                Id = id;
            }
        }
        public sealed class Handler : IRequestHandler<Query, Tag>
        {
            private readonly ITagRepository _repository;

            public Handler(ITagRepository repository)
            {
                _repository = repository;
            }

            public async Task<Tag> Handle(Query request, CancellationToken cancellationToken)
            {
                return await _repository.FindByIdAsync(request.Id);
            }
        }

    }
}
