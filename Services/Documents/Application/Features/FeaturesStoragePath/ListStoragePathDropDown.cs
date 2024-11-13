using Application.Dtos.StoragePath;
using Application.Dtos.Tag;
using Application.Parameters;
using Application.Respository;
using DD.Core.Pagination;
using MediatR;


namespace Application.Features.FeaturesStoragePath
{
    public class ListStoragePathDropDown
    {
        public sealed record Query : IRequest<PagedList<StoragePathDto>>
        {
            public StoragePathParameters storagepathparameters { get; set; }
            public string? Owner { get; set; }
            public Query(string? owner, StoragePathParameters storagepathparam)
            {
                Owner = owner;
                storagepathparameters = storagepathparam;

            }
        }
        public sealed class Handler : IRequestHandler<Query, PagedList<StoragePathDto>>
        {
            private readonly IStoragePathRepository _repository;

            public Handler(IStoragePathRepository repository)
            {
                _repository = repository;
            }

            public async Task<PagedList<StoragePathDto>> Handle(Query request, CancellationToken cancellationToken)
            {
             
                return await _repository.ListStoragePathDropDown<StoragePathDto>(request.storagepathparameters, request.Owner, cancellationToken);
            }
        }

    }
}
