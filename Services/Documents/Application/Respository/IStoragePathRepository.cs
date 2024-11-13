
using Application.Dtos.SelectionData;
using Application.Dtos.StoragePath;
using Application.Features.FeaturesStoragePath;
using Application.Parameters;
using Core.Database;
using DD.Core.Pagination;
using Domain.DocumentManagement.StoragePath;


namespace Application.Respository
{
    public interface IStoragePathRepository:  IRepository<StoragePath, Guid>
    {
        Task<PagedList<StoragePath>> ListStoragePathDropDown<StoragePath>(StoragePathParameters storagepathparameters, string owner, CancellationToken cancellationToken = default);

       
        Task<PagedList<StoragePathDto>> GetPagedStoragePathAsync<StoragePathDto>(StoragePathParameters storagepathparameters, string owner, List<string> GroupsList, CancellationToken cancellationToken = default);
        Task<PagedList<ListStoragePathDto>> GetPagedStoragePathByNameAsync(string name,StoragePathParameters storagepathparameters, string owner, CancellationToken cancellationToken = default);

        Task<List<ListStoragePathDto>> GetStoragePathDetailsAsync<ListStoragePathDto>(CancellationToken cancellationToken = default);
        Task<List<StoragePathDto>> GetListStoragePathByOwner(string owner);
        Task<List<DetailsDTO>> SelectionDataStoragePath(SelectionDataDocuments Ids);
    }
}
