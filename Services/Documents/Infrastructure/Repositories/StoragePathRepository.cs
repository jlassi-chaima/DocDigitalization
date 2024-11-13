using Application.Dtos.Permission;
using Application.Dtos.SelectionData;
using Application.Dtos.StoragePath;
using Application.Exceptions;
using Application.Parameters;
using Application.Respository;
using DD.Core.Pagination;
using Domain.DocumentManagement.StoragePath;
using Domain.DocumentManagement.tags;
using Infrastructure.Persistence;
using Infrastructure.Serializers;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;


namespace Infrastructure.Repositories
{
    public class StoragePathRepository : IStoragePathRepository
    {
        private readonly DBContext _context;

        public StoragePathRepository(DBContext context)
        {
            _context = context;
        }
        public Task AddAsync(StoragePath entity, CancellationToken cancellationToken = default)
        {
            try
            {
                _context.AddAsync(entity, cancellationToken);
                _context.SaveChanges();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
            return Task.FromResult(new StoragePath());
        }

        public Task AddRangeAsync(IEnumerable<StoragePath> entities, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(Expression<Func<StoragePath, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(StoragePath entity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var storagepath = await _context.StoragePaths.FindAsync(id, cancellationToken);

                if (storagepath == null)
                {
                    throw new StoragePathException($"StoragePath with ID {id} not found."); // Or a custom exception type
                }

                _context.StoragePaths.Remove(storagepath);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new StoragePathException(ex.Message.ToString());
            }
        }

        public Task DeleteRangeAsync(IReadOnlyList<StoragePath> entities, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _context?.Dispose();
        }

        public Task<bool> ExistsAsync(Expression<Func<StoragePath, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<StoragePath>> FindAsync(Expression<Func<StoragePath, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<StoragePath?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.StoragePaths.FindAsync(id, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new StoragePathException(ex.Message.ToString());
            }
        }

        public Task<StoragePath?> FindOneAsync(Expression<Func<StoragePath, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<IReadOnlyList<StoragePath>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            if (_context == null)
            {
                // Handle the case when _context is null, perhaps by throwing an exception or logging an error.
                throw new StoragePathException("DbContext is not initialized.");
            }

            try
            {
                var StoragePaths = await _context.StoragePaths.ToListAsync(cancellationToken);
                return StoragePaths;
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                throw new StoragePathException($"Error occurred while fetching tags: {ex.Message}");
             
            }
        }
        

        public async Task<PagedList<StoragePathDto>> GetPagedStoragePathAsync<StoragePathDto>(StoragePathParameters storagepathparameters, string owner, List<string> GroupsList, CancellationToken cancellationToken = default)
        {

            List<Guid> guids = new List<Guid>();
            NewtonSoftService serializer = new NewtonSoftService();
            var query = _context.StoragePaths.Include(d => d.Documents).Select(dt => new
            {
                dt.Id,
                dt.Name,
                dt.Path,
                dt.Match,
                dt.Matching_algorithm,
                dt.Slug,
                dt.Is_insensitive,
                dt.Owner,
                Document_count = dt.Documents.Count,
                Permissions = new PermissionDto
                {
                    View = new UserGroupPermission
                    {
                        Users = dt.UsersView,
                        Groups = dt.GroupsView
                    },
                    Change = new UserGroupPermission
                    {
                        Users = dt.UsersChange,
                        Groups = dt.GroupsChange
                    }
                }
            }).AsQueryable();
            if (!string.IsNullOrEmpty(owner))
            {
                query = query.Where(d => d.Owner.ToLower().Contains(owner.ToLower()) || d.Permissions.View.Users.Any(d => d.Contains(owner)) || d.Permissions.Change.Users.Any(d => d.Contains(owner)) || d.Permissions.View.Groups.Any(group => GroupsList.Contains(group)) || d.Permissions.Change.Groups.Any(group => GroupsList.Contains(group)));
            }
            foreach (var document in query)
            {
                guids.Add(document.Id);
            }
            // Count the total number of document Types
            int count = await query.CountAsync();
            // Get the paged document Types
            var items = await query.Skip(storagepathparameters.PageSize * (storagepathparameters.Page - 1))
                                   .Take(storagepathparameters.PageSize)
                                   .ToListAsync();
            // Serialize the paged documents to a JSON string
            string serializedItems = serializer.Serialize(items);
            // Deserialize the JSON string to a list of document Types
            List<StoragePathDto> deserializedlist = serializer.Deserialize<List<StoragePathDto>>(serializedItems);



            return new PagedList<StoragePathDto>(deserializedlist, count, storagepathparameters.Page, storagepathparameters.PageSize, guids);
        }

        public async Task<PagedList<ListStoragePathDto>> GetPagedStoragePathByNameAsync(string name,StoragePathParameters storagepathparameters, string owner, CancellationToken cancellationToken = default)
        {
            List<Guid> guids = new List<Guid>();
            NewtonSoftService serializer = new NewtonSoftService();
            var query = _context.StoragePaths.Include(d => d.Documents).Select(dt => new
            {
                dt.Id,
                dt.Name,
                dt.Path,
                dt.Match,
                dt.Matching_algorithm,
                dt.Slug,
                dt.Is_insensitive,
                dt.Owner,
                Document_count = dt.Documents.Count,

            }).Where(storagepath => storagepath.Name.Contains(name) && storagepath.Owner == owner)
                                        .AsQueryable();
            // Count the total number of document Types
            int count = await query.CountAsync();
            // Get the paged document Types
            var items = await query.Skip(storagepathparameters.PageSize * (storagepathparameters.Page - 1))
                                   .Take(storagepathparameters.PageSize)
                                   .ToListAsync();
            // Serialize the paged documents to a JSON string
            string serializedItems = serializer.Serialize(items);
            // Deserialize the JSON string to a list of document Types
            List<ListStoragePathDto> deserializedlist = serializer.Deserialize<List<ListStoragePathDto>>(serializedItems);



            return new PagedList<ListStoragePathDto>(deserializedlist, count, storagepathparameters.Page, storagepathparameters.PageSize, guids);

        }

        public async Task<List<ListStoragePathDto>> GetStoragePathDetailsAsync<ListStoragePathDto>(CancellationToken cancellationToken = default)
        {
            NewtonSoftService serializer = new NewtonSoftService();
            var StoragePaths = await _context.StoragePaths.ToListAsync(cancellationToken);
            string serializedItems = serializer.Serialize(StoragePaths);
            List<ListStoragePathDto> StoragePathlist = serializer.Deserialize<List<ListStoragePathDto>>(serializedItems);


            return StoragePathlist;
        }

        public async Task<PagedList<StoragePath>> ListStoragePathDropDown<StoragePath>(StoragePathParameters storagepathparameters, string owner, CancellationToken cancellationToken = default)
        {
            List<Guid> guids = new List<Guid>();
            NewtonSoftService serializer = new NewtonSoftService();
            var query = _context.StoragePaths.Include(d => d.Documents).Select(dt => new
            {
                dt.Id,
                dt.Name,
                dt.Path,
                dt.Match,
                dt.Matching_algorithm,
                dt.Slug,
                dt.Is_insensitive,
                dt.Owner,
                Document_count = dt.Documents.Count,
                Permissions = new PermissionDto
                {
                    View = new UserGroupPermission
                    {
                        Users = dt.UsersView,
                        Groups = dt.GroupsView
                    },
                    Change = new UserGroupPermission
                    {
                        Users = dt.UsersChange,
                        Groups = dt.GroupsChange
                    }
                }
            }).AsQueryable();
            if (!string.IsNullOrEmpty(owner))
            {
                query = query.Where(d => d.Owner.ToLower().Contains(owner.ToLower()));
            }
            foreach (var document in query)
            {
                guids.Add(document.Id);
            }
            // Count the total number of document Types
            int count = await query.CountAsync();
            // Get the paged document Types
            var items = await query.Skip(storagepathparameters.PageSize * (storagepathparameters.Page - 1))
                                   .Take(storagepathparameters.PageSize)
                                   .ToListAsync();
            // Serialize the paged documents to a JSON string
            string serializedItems = serializer.Serialize(items);
            // Deserialize the JSON string to a list of document Types
            List<StoragePath> deserializedlist = serializer.Deserialize<List<StoragePath>>(serializedItems);



            return new PagedList<StoragePath>(deserializedlist, count, storagepathparameters.Page, storagepathparameters.PageSize, guids);

        }

        public async Task<List<DetailsDTO>> SelectionDataStoragePath(SelectionDataDocuments Ids)
        {
            // Convert documentIds to Guids if your Documents contain GUIDs
            var documentIds = Ids.Documents.Select(Guid.Parse).ToList();

            var query = await _context.StoragePaths
                .Include(sp => sp.Documents)
              //  .Where(sp => sp.Documents.Any(d => documentIds.Contains(d.Id)))
                .Select(sp => new DetailsDTO
                {
                    Id = sp.Id,
                    Document_count = sp.Documents.Where(d => documentIds.Contains(d.Id)).Count()
                })
                .ToListAsync();


            return query;
        }

        public async Task UpdateAsync(StoragePath entity, CancellationToken cancellationToken = default)
        {
            try
            {
                var existingEntity = await _context.StoragePaths.FindAsync(entity.Id);

                if (existingEntity == null)
                {
                    throw new StoragePathException($"StoragePath with ID {entity.Id} not found.");
                }

                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new StoragePathException(ex.Message.ToString());
            }
        }

        public async Task<List<StoragePathDto>> GetListStoragePathByOwner(string owner)
        {

            if (_context == null)
            {
                // Handle the case when _context is null, perhaps by throwing an exception or logging an error.
                throw new InvalidOperationException("DbContext is not initialized.");
            }

            try
            {
                var query =await _context.StoragePaths.Include(d => d.Documents).Select(dt => new StoragePathDto
                {
                    Id =dt.Id,
                    Name= dt.Name,
                    Path=  dt.Path,
                    Match=dt.Match,
                    Matching_algorithm= dt.Matching_algorithm,
                    Slug= dt.Slug,
                    Is_insensitive=   dt.Is_insensitive,
                    Owner=    dt.Owner,
                    Document_count = dt.Documents.Count,
                  
                }).Where(storage => storage.Owner == owner)
                                .ToListAsync();
                return query;
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                Console.WriteLine($"Error occurred while fetching tags: {ex.Message}");
                throw; // Re-throw the exception to propagate it upwards
            }
        }
    }
}
