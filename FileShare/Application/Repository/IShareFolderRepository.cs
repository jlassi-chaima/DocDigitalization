using Application.Dtos.ShareFolder;
using Application.PaginationParams;
using Core.Database;
using DD.Core.Pagination;
using Domain.FileShare;

namespace Application.Repository
{
    public interface IShareFolderRepository : IRepository<ShareFolder, Guid>
    {
        Task<ShareFolder> GetByFolderPathAsync(string folderPath);
        Task UpdateLastWriteTimeAsync(string folderPath, DateTime lastWriteTime);
        Task<PagedList<ShareFolderPagedList>> GetPagedFileShareAsync(ShareFolderParameters sharefolderparameters,string owner, Guid groupId,CancellationToken cancellationToken = default);
    }
}
