using Application.Dtos.ShareFolder;
using Application.Exceptions;
using Application.PaginationParams;
using Application.Repository;
using DD.Core.Pagination;
using Domain.FileShare;
using Infrastructure.Persistence;
using Infrastructure.Serializers;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;


namespace Infrastructure.Repositories
{
    public class ShareFolderRepository : IShareFolderRepository
    {
        private readonly DBContext _context;

        public ShareFolderRepository(DBContext context)
        {
            _context = context;
        }
        public async Task AddAsync(ShareFolder entity, CancellationToken cancellationToken = default)
        {
            try
            {
                await _context.AddAsync(entity, cancellationToken);
                _context.SaveChanges();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
        }

        public Task AddRangeAsync(IEnumerable<ShareFolder> entities, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(Expression<Func<ShareFolder, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(ShareFolder entity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var ShareFolder = await _context.FileShares.FindAsync(id, cancellationToken);

            if (ShareFolder == null)
            {
                throw new ShareFolderException($"ShareFolder with ID {id} not found.");
            }

            _context.FileShares.Remove(ShareFolder);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public Task DeleteRangeAsync(IReadOnlyList<ShareFolder> entities, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _context?.Dispose();
        }

        public Task<bool> ExistsAsync(Expression<Func<ShareFolder, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<ShareFolder>> FindAsync(Expression<Func<ShareFolder, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<ShareFolder?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.FileShares.FindAsync(id, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new ShareFolderException(ex.Message.ToString());
            }
        }

        public Task<ShareFolder?> FindOneAsync(Expression<Func<ShareFolder, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<IReadOnlyList<ShareFolder>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var fileshares = await _context.FileShares.ToListAsync(cancellationToken);
            return fileshares;
        }

        public async Task<ShareFolder> GetByFolderPathAsync(string folderPath)
        {
            var sharefolder = await _context.FileShares.FirstOrDefaultAsync(path => path.FolderPath == folderPath);
            if (sharefolder == null)
            {
                throw new ShareFolderException($"Share folder with path '{folderPath}' does not exist.");
            }

            return sharefolder;
        }

        public async Task<PagedList<ShareFolderPagedList>> GetPagedFileShareAsync(ShareFolderParameters sharefolderparameters,string owner,Guid groupId, CancellationToken cancellationToken = default)
        {
            List<Guid> guids = new List<Guid>();
            NewtonSoftService serializer = new NewtonSoftService();

            var query = _context.FileShares.Where(s=>s.Owner==owner && s.GroupId==groupId).AsQueryable();
            foreach (var tag in query)
            {
                guids.Add(tag.Id);
            }
            int count = await query.CountAsync();

            var items = await query
                .Skip(sharefolderparameters.PageSize * (sharefolderparameters.Page - 1))
                .Take(sharefolderparameters.PageSize)
                .ToListAsync();

            string serializedItems = serializer.Serialize(items);
            List<ShareFolderPagedList> deserializedItems1 = serializer.Deserialize<List<ShareFolderPagedList>>(serializedItems);
            return new PagedList<ShareFolderPagedList>(deserializedItems1, count, sharefolderparameters.Page, sharefolderparameters.PageSize, guids);
        }

        public async Task UpdateAsync(ShareFolder entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
            {
                throw new ShareFolderException($"Correspondent with ID {entity.Id} not found.");
            }
            try
            {
                _context.Update(entity);
                await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new ShareFolderException("An error occurred while updating the entity.");
            }
        }

        public async Task UpdateLastWriteTimeAsync(string folderPath, DateTime lastWriteTime)
        {
            var shareFolder = await _context.FileShares.FirstOrDefaultAsync(f => f.FolderPath == folderPath);

            if (shareFolder == null)
            {
                throw new Exception($"Share folder with path '{folderPath}' does not exist.");
            }

            shareFolder.CreationTime = lastWriteTime;

            // Save changes to the database
            await _context.SaveChangesAsync();
        }
    }
}
