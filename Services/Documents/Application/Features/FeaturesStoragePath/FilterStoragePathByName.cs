
using Application.Dtos.StoragePath;
using Application.Parameters;
using Application.Respository;
using DD.Core.Pagination;
using MediatR;

namespace Application.Features.FeaturesStoragePath
{
    public class FilterStoragePathByName
    {
        public sealed record Query : IRequest<PagedList<ListStoragePathDto>>
        {
            public StoragePathParameters Storagepathparameters;
            public  string Name;
            public string Owner;
            public Query(string namefilter, StoragePathParameters storagepathparams, string owner)
            {
                Name = namefilter;
                Storagepathparameters = storagepathparams;
                Owner = owner;
            }
        }
        public class Handler : IRequestHandler<Query, PagedList<ListStoragePathDto>>
        {
            private readonly IStoragePathRepository _stroagepathRepository;

            public Handler(IStoragePathRepository stroagepathRepository)
            {
                _stroagepathRepository = stroagepathRepository;
            }

            public async Task<PagedList<ListStoragePathDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                 return await _stroagepathRepository.GetPagedStoragePathByNameAsync(request.Name, request.Storagepathparameters, request.Owner);
            }
        }
    }
}
