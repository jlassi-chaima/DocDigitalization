using Application.Dtos.Correspondent;
using Application.Dtos.Permission;
using Application.Dtos.SelectionData;
using Application.Dtos.Tag;
using Application.Exceptions;
using Application.Parameters;
using Application.Respository;
using Core.Exceptions;
using DD.Core.Pagination;
using Domain.DocumentManagement.Correspondent;
using Infrastructure.Persistence;
using Infrastructure.Serializers;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Linq.Expressions;


namespace Infrastructure.Repositories
{
    public class CorrespondentRepository : ICorrespondentRepository
    {
        private readonly DBContext _context;

        public CorrespondentRepository(DBContext context)
        {
            _context = context;
        }
        public Task AddAsync(Correspondent entity, CancellationToken cancellationToken = default)
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
            return Task.FromResult(new Correspondent());
        }

        public Task AddRangeAsync(IEnumerable<Correspondent> entities, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteAsync(Expression<Func<Correspondent, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var correspondentsToDelete = await _context.Correspondents.Where(predicate).ToListAsync(cancellationToken);

            foreach (var correspondent in correspondentsToDelete)
            {
                _context.Correspondents.Remove(correspondent);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Correspondent entity, CancellationToken cancellationToken = default)
        {
            try
            {
                _context.Correspondents.Remove(entity);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new CorrespondentException(ex.Message.ToString());
            }
        }

        public async Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var correspondent = await _context.Correspondents.FindAsync(id, cancellationToken);

                if (correspondent == null)
                {
                    throw new CorrespondentException($"Correspondent with ID {id} not found."); // Or a custom exception type
                }

                _context.Correspondents.Remove(correspondent);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new CorrespondentException(ex.Message.ToString());
            }
        }

        public async Task DeleteRangeAsync(IReadOnlyList<Correspondent> entities, CancellationToken cancellationToken = default)
        {
            try
            {
                foreach (var entity in entities)
                {
                    _context.Correspondents.Remove(entity);
                }

                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new CorrespondentException(ex.Message.ToString());
            }
        }

        public void Dispose()
        {
            _context?.Dispose();
        }

        public async Task<bool> ExistsAsync(Expression<Func<Correspondent, bool>> predicate, CancellationToken cancellationToken = default)
        {
            bool correspondentExists = await _context.Correspondents.AnyAsync(predicate, cancellationToken);

            return correspondentExists;
        }

        public async Task<IReadOnlyList<Correspondent>> FindAsync(Expression<Func<Correspondent, bool>> predicate, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Correspondents.Where(predicate).ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new CorrespondentException(ex.Message.ToString());
            }
        }
        public async Task<Correspondent> FindByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
           return await _context.Correspondents.Where(d=>d.Name == username).FirstOrDefaultAsync(cancellationToken);
        }
        public async Task<Correspondent?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Correspondents.FindAsync(id, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new CorrespondentException(ex.Message.ToString());
            }
        }

        public Task<Correspondent?> FindOneAsync(Expression<Func<Correspondent, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
        public async Task<List<CorrespondentListDTO>> GetListStoragePathByOwner(string owner)
        {
            if (_context == null)
            {
                // Handle the case when _context is null, perhaps by throwing an exception or logging an error.
                throw new InvalidOperationException("DbContext is not initialized.");
            }

            try
            {
                var query = await _context.Correspondents.Include(d => d.Documents).Select(dt => new CorrespondentListDTO
                {
                    Id = dt.Id,
                    Name = dt.Name,
                    Match = dt.Match,
                    Matching_algorithm = dt.Matching_algorithm,
                    Is_insensitive = dt.Is_insensitive,
                    Owner = dt.Owner,
                    Document_count = dt.Documents.Count,

                }).Where(doctype => doctype.Owner == owner)
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
        public async Task<IReadOnlyList<Correspondent>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var correspondents = await _context.Correspondents.ToListAsync(cancellationToken).ConfigureAwait(false);

            return correspondents;
        }

        public async Task<List<CorrespondentListDTO>> GetCorrespondentDetailsAsync<CorrespondentListDTO>( CancellationToken cancellationToken = default)
        {
            NewtonSoftService serializer = new NewtonSoftService();
            var correspondents = await _context.Correspondents.ToListAsync(cancellationToken).ConfigureAwait(false);
            string serializedCorrespondent = serializer.Serialize(correspondents);
            List<CorrespondentListDTO> CorrespondentList = serializer.Deserialize<List<CorrespondentListDTO>>(serializedCorrespondent);
            return CorrespondentList;
        }

        public async Task<PagedList<CorrespondentListDTO>> GetCorrespondentsByNameAsync(CorrespondentParameters correspondentparameters,string name,string owner, CancellationToken cancellationToken = default)
        {
            List<Guid> guids = new List<Guid>();
            NewtonSoftService serializer = new NewtonSoftService();
            try
            {
                var correspondents =  _context.Correspondents.Select(t => new{
                                                                         t.Id,
                                                                         t.Name,
                                                                         t.Slug,
                                                                         t.Match,
                                                                         t.Matching_algorithm,
                                                                         t.Last_correspondence,
                                                                         t.Is_insensitive,
                                                                         t.Owner,
                                                                         Document_Count = t.Documents.Count
                                                                         })
                                          .Where(correspondent => correspondent.Name.Contains(name) && correspondent.Owner == owner)
                                         .AsQueryable();
                foreach (var correspondent  in correspondents)
                {
                    guids.Add(correspondent.Id);
                }
                int count = await correspondents.CountAsync();

                var items = await correspondents
                    .Skip(correspondentparameters.PageSize * (correspondentparameters.Page - 1))
                    .Take(correspondentparameters.PageSize)
                    .ToListAsync();

                string serializedCorrespondent = serializer.Serialize(items);
                List<CorrespondentListDTO> deserializedCorrespondent = serializer.Deserialize<List<CorrespondentListDTO>>(serializedCorrespondent);
                return new PagedList<CorrespondentListDTO>(deserializedCorrespondent, count, correspondentparameters.Page, correspondentparameters.PageSize, guids);
            }
            catch (Exception ex)
            {
                throw new CorrespondentException(ex.Message.ToString());

            }
        }

        public async  Task<PagedList<CorrespondentListDTO>> GetPagedCorrespondentAsync<CorrespondentListDTO>(CorrespondentParameters correspondentparameters,string? owner, List<string>? GroupsList, CancellationToken cancellationToken = default)
        {
            List<Guid> guids = new List<Guid>();
            NewtonSoftService serializer = new NewtonSoftService();
            var query = _context.Correspondents.Select(t => new
            {
                t.Id,
                t.Name,
                t.Slug,
                t.Match,
                t.Matching_algorithm,
                t.Last_correspondence,
                t.Is_insensitive,
                t.Owner,
                Document_Count = t.Documents.Count,
                Permissions = new PermissionDto
                {
                    View = new UserGroupPermission
                    {
                        Users = t.UsersView,
                        Groups = t.GroupsView
                    },
                    Change = new UserGroupPermission
                    {
                        Users = t.UsersChange,
                        Groups = t.GroupsChange
                    }
                }
            }).AsQueryable();
            if(!string.IsNullOrEmpty(owner))
            {
                query = query.Where(d => d.Owner.ToLower().Contains(owner.ToLower()) || d.Permissions.View.Users.Any(d => d.Contains(owner)) || d.Permissions.Change.Users.Any(d => d.Contains(owner)) || d.Permissions.View.Groups.Any(group => GroupsList.Contains(group)) || d.Permissions.Change.Groups.Any(group => GroupsList.Contains(group)));
            }
            foreach (var correspondent in query)
            {
                guids.Add(correspondent.Id);
            }
            int count = await query.CountAsync();
            var items = await query
            .Skip(correspondentparameters.PageSize * (correspondentparameters.Page - 1))
            .Take(correspondentparameters.PageSize)
                .ToListAsync();
            string serializedCorrespondent = serializer.Serialize(items);
            List<CorrespondentListDTO> deserializedCorrespondent = serializer.Deserialize<List<CorrespondentListDTO>>(serializedCorrespondent);
            return new PagedList<CorrespondentListDTO>(deserializedCorrespondent, count, correspondentparameters.Page, correspondentparameters.PageSize,guids);
        }

        public async Task<List<DetailsDTO>> SelectionDataCorrespondent(SelectionDataDocuments ids, string owner)
        {
            var documentIds = ids.Documents;
            var test = await _context.Correspondents.Include(d => d.Documents).ToListAsync();
            var query = await _context.Correspondents.Include(d=>d.Documents).Where(c => c.Owner == owner )
                                                                              .Select(t => new DetailsDTO
            {
                Id = t.Id,
                Document_count = t.Documents.Where(d => documentIds.Contains(d.Id.ToString())).Count()
            }).ToListAsync();

            return query;
        }

        public async Task UpdateAsync(Correspondent entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
            {
                throw new CorrespondentException($"Correspondent with ID {entity.Id} not found.");
            }
            try
            {
                _context.Update(entity);
                await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
           
            catch (CorrespondentException ex)
            {
                Log.Error(ex.Message);
                throw new CorrespondentException(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new CorrespondentException($"An error occurred while updating the entity.Error message: {ex.Message}");
            }
        }
        public async Task<bool> ExistsByIdAsync(Guid correspondentId, CancellationToken cancellationToken = default)
        {
            return await ExistsAsync(corr => corr.Id == correspondentId, cancellationToken);
        }
    }
}
