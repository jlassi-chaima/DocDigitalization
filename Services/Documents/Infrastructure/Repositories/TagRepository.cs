using Application.Exceptions;
using Application.Respository;
using DD.Core.Pagination;
using Domain.DocumentManagement.tags;
using Infrastructure.Persistence;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Application.Parameters;
using Infrastructure.Serializers;
using Application.Dtos.Tag;
using Newtonsoft.Json;
using Application.Dtos.StoragePath;
using Application.Dtos.SelectionData;
using System.Linq;
using NPOI.SS.Formula.Functions;
using Application.Dtos.Permission;
using System.Threading;
namespace Infrastructure.Repositories
{
    public class TagRepository : ITagRepository
    {
        private readonly DBContext _context;
        private readonly Mapper _mapper;
        public TagRepository(DBContext context, Mapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public Task AddAsync(Tag entity, CancellationToken cancellationToken = default)
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
            return Task.FromResult(new Tag());
        }
        public async Task AddRangeAsync(IEnumerable<Tag> entities, CancellationToken cancellationToken = default)
        {
            try
            {
                await _context.AddRangeAsync(entities, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw; // Rethrow the exception to propagate it up the call stack
            }
        }
        public Task DeleteAsync(Expression<Func<Tag, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(Tag entity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var tag = await _context.Tags.FindAsync(id, cancellationToken);

                if (tag == null)
                {
                    throw new TagException($"Tag with ID {id} not found."); // Or a custom exception type
                }

                _context.Tags.Remove(tag);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new TagException(ex.Message.ToString());
            }
        }

        public Task DeleteRangeAsync(IReadOnlyList<Tag> entities, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _context?.Dispose(); 
        }

        public Task<bool> ExistsAsync(Expression<Func<Tag, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<Tag>> FindAsync(Expression<Func<Tag, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<Tag?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
               
                    var tag = await _context.Tags
                        .FindAsync(id, cancellationToken);

                    if (tag != null)
                    {
                        await _context.Entry(tag)
                            .Collection(d => d.Documents)
                            .LoadAsync(cancellationToken);
                    }

                    return tag;
                }
            catch (Exception ex)
            {
                throw new TagException(ex.Message.ToString());
            }
        }

        public async Task<List<Tag>> FindByListIdsAsync(IEnumerable<Guid> tagIds, CancellationToken cancellationToken = default)
        {
            return await _context.Tags.Where(tag => tagIds.Contains(tag.Id)).ToListAsync(cancellationToken);
        }

        public Task<Tag?> FindOneAsync(Expression<Func<Tag, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<IReadOnlyList<Tag>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            if (_context == null)
            {
                // Handle the case when _context is null, perhaps by throwing an exception or logging an error.
                throw new InvalidOperationException("DbContext is not initialized.");
            }

            try
            {
                var tags = await _context.Tags.ToListAsync(cancellationToken);
                return tags;
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                Console.WriteLine($"Error occurred while fetching tags: {ex.Message}");
                throw; // Re-throw the exception to propagate it upwards
            }
        }

        public async Task<List<Tag>> GetListTagsByOwner(string owner)
        {
            if (_context == null)
            {
                // Handle the case when _context is null, perhaps by throwing an exception or logging an error.
                throw new InvalidOperationException("DbContext is not initialized.");
            }

            try
            {
                var tags = await _context.Tags
                                .Where(tag => tag.Owner == owner)
                                .ToListAsync();
                return tags;
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                Console.WriteLine($"Error occurred while fetching tags: {ex.Message}");
                throw; // Re-throw the exception to propagate it upwards
            }
        }

        public async Task<PagedList<TagDtoDetails>>  GetPagedtagAsync<TagDtoDetails>(TagParameters tagparameters, string owner,List<string> GroupsList, CancellationToken cancellationToken = default)
        {
            List<Guid> guids = new List<Guid>();
            NewtonSoftService serializer = new NewtonSoftService();

            var query = _context.Tags.Select(t=>new
            {
                t.Id,
                t.Name,
                t.Slug,
                t.Color,
                t.Match,
                t.Matching_algorithm,
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

            foreach (var tag in query)
            {
                guids.Add(tag.Id);
            }
            int count = await query.CountAsync();
           
            var items = await query
                .Skip(tagparameters.PageSize * (tagparameters.Page - 1))
                .Take(tagparameters.PageSize)
                .ToListAsync();
          
            string serializedItems = serializer.Serialize(items);
            List<TagDtoDetails> deserializedItems1 = serializer.Deserialize<List<TagDtoDetails>>(serializedItems);
            return new PagedList<TagDtoDetails>(deserializedItems1, count, tagparameters.Page, tagparameters.PageSize, guids);
        }

        public async Task<List<TagDtoDetails>> GetTagDetailsAsync(CancellationToken cancellationToken = default)
        {
            NewtonSoftService serializer = new NewtonSoftService();
            var tags = await _context.Tags.ToListAsync(cancellationToken);
            string serializedItems = serializer.Serialize(tags);
            List<TagDtoDetails> Tagslist = serializer.Deserialize<List<TagDtoDetails>>(serializedItems);


          
            return Tagslist;
        }

        public async  Task<PagedList<TagDtoDetails>> GetTagsByNameAsync(TagParameters tagparameters, string name, string owner, CancellationToken cancellationToken = default)
        {
            List<Guid> guids = new List<Guid>();
            NewtonSoftService serializer = new NewtonSoftService();
            try
            {
                var query = _context.Tags.Select(t => new
                {
                    t.Id,
                    t.Name,
                    t.Slug,
                    t.Color,
                    t.Match,
                    t.Matching_algorithm,
                    t.Is_insensitive,
                    t.Owner,
                    Document_Count = t.Documents.Count
                }).Where(tag => tag.Name.Contains(name) && tag.Owner == owner)
                                        .AsQueryable();
                foreach (var tag in query)
                {
                    guids.Add(tag.Id);
                }
                int count = await query.CountAsync();

                var items = await query
                    .Skip(tagparameters.PageSize * (tagparameters.Page - 1))
                    .Take(tagparameters.PageSize)
                    .ToListAsync();

                string serializedItems = serializer.Serialize(items);
                List<TagDtoDetails> deserializedlisttags = serializer.Deserialize<List<TagDtoDetails>>(serializedItems);
                return new PagedList<TagDtoDetails>(deserializedlisttags, count, tagparameters.Page, tagparameters.PageSize, guids);

            }
            catch (Exception ex)
            {
                throw new TagException(ex.Message.ToString());
                
            }
        }

        public async Task<List<DetailsDTO>> SelectionDataTag(SelectionDataDocuments Ids, string owner)
        {
            var documentIds = Ids.Documents;

            var query = await _context.Tags.Include(t => t.Documents)
                                           .Where(t => t.Owner == owner)
                                           .Select(t => new DetailsDTO
                                           {
                                               Id = t.Id,
                                               Document_count = t.Documents.Where(dt=> documentIds.Contains(dt.DocumentId.ToString())).Count()
                                           })
                                           .ToListAsync();

            return query;
        }
        public async Task UpdateAsync(Tag entity, CancellationToken cancellationToken = default)
        {
            NewtonSoftService serializer = new NewtonSoftService();
            try
                {
                    var existingEntity = await _context.Tags.FindAsync(entity.Id);

                    if (existingEntity == null)
                    {
                        throw new TagException($"Tag with ID {entity.Id} not found.");
                    }
                var settings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore // Ignorer les boucles de référence
                                                                         // ou utilisez ReferenceLoopHandling.Serialize pour sérialiser les références circulaires
                };

                // Sérialiser l'entité existante en JSON avec les paramètres configurés
                var json = JsonConvert.SerializeObject(existingEntity, settings);
                await _context.SaveChangesAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    throw new TagException(ex.Message.ToString());
                }
        }
    }
}
