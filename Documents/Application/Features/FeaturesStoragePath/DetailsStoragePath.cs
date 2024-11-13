using Application.Respository;
using Domain.DocumentManagement.StoragePath;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.FeaturesStoragePath
{
    public class DetailsStoragePath
    {
        public sealed record Query : IRequest<StoragePath>
        {
            public readonly Guid Id;
            public Query(Guid id)
            {
                Id = id;
            }
        }
        public sealed class Handler : IRequestHandler<Query, StoragePath>
        {
            private readonly IStoragePathRepository _repository;

            public Handler(IStoragePathRepository repository)
            {
                _repository = repository;
            }

            public async Task<StoragePath> Handle(Query request, CancellationToken cancellationToken)
            {
                return await _repository.FindByIdAsync(request.Id, cancellationToken);
            }
        }
    }
}