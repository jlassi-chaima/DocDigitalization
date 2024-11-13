using Application.Repository;
using Domain.FileShare;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.FeatureShareFolder
{
    public class DetailsShareFolder
    {
        public sealed record Query : IRequest<ShareFolder>
        {
            public readonly Guid Id;
            public Query(Guid id)
            {
                Id = id;
            }
        }
        public sealed class Handler : IRequestHandler<Query, ShareFolder>
        {
            private readonly IShareFolderRepository _repository;

            public Handler(IShareFolderRepository repository)
            {
                _repository = repository;
            }

            public async Task<ShareFolder> Handle(Query request, CancellationToken cancellationToken)
            {
                return await _repository.FindByIdAsync(request.Id, cancellationToken);
            }
        }
    }
}
