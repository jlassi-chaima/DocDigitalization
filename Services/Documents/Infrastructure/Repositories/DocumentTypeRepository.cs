using Application.Dtos.DocumentType;
using Application.Dtos.Permission;
using Application.Dtos.SelectionData;
using Application.Exceptions;
using Application.Parameters;
using Application.Respository;
using DD.Core.Pagination;
using Domain.DocumentManagement;
using Domain.DocumentManagement.DocumentTypes;
using Infrastructure.Persistence;
using Infrastructure.Serializers;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;

namespace Infrastructure.Repositories
{
    public class DocumentTypeRepository : IDocumentTypeRepository
    {
        private readonly DBContext _context;

        public DocumentTypeRepository(DBContext context)
        {
            _context = context;
        }
        public async Task AddAsync(DocumentType entity, CancellationToken cancellationToken = default)
        {
            try
            {

               _context.DocumentTypes.Add(entity);
               await _context.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                throw new DocumentTypeException(ex.Message.ToString());
            }
           
        }

        public Task AddRangeAsync(IEnumerable<DocumentType> entities, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
        public async Task<List<DocumentTypeDetailsDTO>> GetDocumentTypesDetailsWithMatchingAlgoAutoAsync(CancellationToken cancellationToken = default)
        {
            NewtonSoftService serializer = new NewtonSoftService();
            var documents = await _context.DocumentTypes.Where(d => d.Matching_algorithm == Matching_Algorithms.MATCH_AUTO).ToListAsync(cancellationToken).ConfigureAwait(false);
            string serializedItems = serializer.Serialize(documents);
            List<DocumentTypeDetailsDTO> documentstypelist = serializer.Deserialize<List<DocumentTypeDetailsDTO>>(serializedItems);
            return documentstypelist;

        }
        public async Task DeleteAsync(Expression<Func<DocumentType, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var documenttypeToDelete = await _context.DocumentTypes.Where(predicate).ToListAsync(cancellationToken);

            foreach (var document in documenttypeToDelete)
            {
                _context.DocumentTypes.Remove(document);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
        public async Task<List<DocumentTypeDetailsDTO>> GetListDocumentTypeByOwner(string owner)
        {

            if (_context == null)
            {
                // Handle the case when _context is null, perhaps by throwing an exception or logging an error.
                throw new InvalidOperationException("DbContext is not initialized.");
            }

            try
            {
                var query = await _context.DocumentTypes.Include(d => d.Documents).Select(dt => new DocumentTypeDetailsDTO
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
       
        public async Task DeleteAsync(DocumentType entity, CancellationToken cancellationToken = default)
        {
            try
            {
                _context.DocumentTypes.Remove(entity);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new DocumentTypeException(ex.Message.ToString());
            }
        }

        public async Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var document = await _context.DocumentTypes.FindAsync(id, cancellationToken);

                if (document == null)
                {
                    throw new DocumentTypeException($"Document Type with ID {id} not found."); // Or a custom exception type
                }

                _context.DocumentTypes.Remove(document);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new DocumentTypeException(ex.Message.ToString());
            }
        }

        public async Task DeleteRangeAsync(IReadOnlyList<DocumentType> entities, CancellationToken cancellationToken = default)
        {
            try
            {
                foreach (var entity in entities)
                {
                    _context.DocumentTypes.Remove(entity);
                }

                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new DocumentTypeException(ex.Message.ToString());
            }
        }

        public void Dispose()
        {
            _context?.Dispose();
        }

        public async Task<bool> ExistsAsync(Expression<Func<DocumentType, bool>> predicate, CancellationToken cancellationToken = default)
        {
            bool documentExists = await _context.DocumentTypes.AnyAsync(predicate, cancellationToken);

            return documentExists;
        }

        public async Task<IReadOnlyList<DocumentType>> FindAsync(Expression<Func<DocumentType, bool>> predicate, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.DocumentTypes.Where(predicate).ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new DocumentTypeException(ex.Message.ToString());
            }
        }
      

        public async Task<DocumentType?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.DocumentTypes.FindAsync(id, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new DocumentTypeException(ex.Message.ToString());
            }
        }
        public async Task<DocumentType?> FindByName(string name, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.DocumentTypes.FirstOrDefaultAsync(x=>x.Name==name ,cancellationToken);
            }
            catch (Exception ex)
            {
                throw new DocumentTypeException(ex.Message.ToString());
            }
        }

        public Task<DocumentType?> FindOneAsync(Expression<Func<DocumentType, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<IReadOnlyList<DocumentType>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var documents = await _context.DocumentTypes.ToListAsync(cancellationToken).ConfigureAwait(false);

            return documents;
        }
        public async Task<List<DocumentTypeDetailsDTO>> GetDocumentTypesDetailsAsync(CancellationToken cancellationToken = default)
        {
            NewtonSoftService serializer = new NewtonSoftService();
            var documents = await _context.DocumentTypes.ToListAsync(cancellationToken).ConfigureAwait(false);
            string serializedItems = serializer.Serialize(documents);
            List<DocumentTypeDetailsDTO> documentstypelist = serializer.Deserialize<List<DocumentTypeDetailsDTO>>(serializedItems);


            return documentstypelist;
        }
            public async Task<PagedList<DocumentTypeDetailsDTO>> GetDocumentTypesByNameAsync(string name, DocumentTypeParameters documenttypeparameters, string owner, CancellationToken cancellationToken = default)
        {
            try
            {
                List<Guid> guids = new List<Guid>();
                NewtonSoftService serializer = new NewtonSoftService();
                var query = _context.DocumentTypes.Include(d => d.Documents).Select(dt => new
                {
                    dt.Id,
                    dt.Name,
                    dt.Slug,
                    dt.Match,
                    dt.Matching_algorithm,
                    dt.Is_insensitive,
                    dt.Owner,
                    Document_Count = dt.Documents.Count
                }).Where(documenttype => documenttype.Name.Contains(name) && documenttype.Owner == owner)
                                        .AsQueryable();

                foreach (var document in query)
                {
                    guids.Add(document.Id);
                }
                // Count the total number of document Types
                int count = await query.CountAsync();
                // Get the paged document Types
                var items = await query.Skip(documenttypeparameters.PageSize * (documenttypeparameters.Page - 1))
                                       .Take(documenttypeparameters.PageSize)
                                       .ToListAsync();
                // Serialize the paged documents to a JSON string
                string serializedItems = serializer.Serialize(items);
                // Deserialize the JSON string to a list of document Types
                List<DocumentTypeDetailsDTO> deserializedlist = serializer.Deserialize<List<DocumentTypeDetailsDTO>>(serializedItems);



                return new PagedList<DocumentTypeDetailsDTO>(deserializedlist, count, documenttypeparameters.Page, documenttypeparameters.PageSize, guids);
            }
            catch (Exception ex)
            {
                throw new TagException(ex.Message.ToString());

            }
        }

        public async Task<PagedList<DocumentTypeDetailsDTO>> GetPagedDocumentTypeAsync<DocumentTypeDetailsDTO>(DocumentTypeParameters documenttypeparameters, string owner, List<string>? GroupsList, CancellationToken cancellationToken = default)
        {
            List<Guid> guids = new List<Guid>();
            NewtonSoftService serializer = new NewtonSoftService();
            var query = _context.DocumentTypes.Include(d=>d.Documents).Select(dt => new
                                                                                        {
                                                                                            dt.Id,
                                                                                            dt.Name,
                                                                                            dt.Slug,
                                                                                            dt.Match,
                                                                                            dt.Matching_algorithm,
                                                                                            dt.Is_insensitive,
                                                                                            dt.Owner,
                                                                                            dt.ExtractedData,
                                                                                            Document_Count = dt.Documents.Count,
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
            var items = await query.Skip(documenttypeparameters.PageSize * (documenttypeparameters.Page - 1))
                                   .Take(documenttypeparameters.PageSize)
                                   .ToListAsync();
            // Serialize the paged documents to a JSON string
            string serializedItems = serializer.Serialize(items);
            // Deserialize the JSON string to a list of document Types
            List<DocumentTypeDetailsDTO> deserializedlist = serializer.Deserialize<List<DocumentTypeDetailsDTO>>(serializedItems);



            return new PagedList<DocumentTypeDetailsDTO>(deserializedlist, count, documenttypeparameters.Page, documenttypeparameters.PageSize,guids);
        }

        public async Task UpdateAsync(DocumentType entity, CancellationToken cancellationToken = default)
        {
            try
            {
                var existingEntity = await _context.DocumentTypes.FindAsync(entity.Id);

                if (existingEntity == null)
                {
                    throw new DocumentTypeException($"Document Type with ID {entity.Id} not found.");
                }

                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new DocumentTypeException(ex.Message.ToString());
            }
        }

        public async Task<List<DetailsDTO>> SelectionDataDocumentTypes(SelectionDataDocuments Ids)
        {

            var documentIds = Ids.Documents;
            //var test = await _context.DocumentTypes.Include(d => d.Documents).ToListAsync();
            //var query = await _context.DocumentTypes.Include(d => d.Documents)
            //   // .Where(c => c.Documents.Any(d => documentIds.Contains(d.Id.ToString())))
            //    .Select(t => new DetailsDTO
            //{
            //    Id = t.Id,
            //    Document_count = t.Documents.Where(d => documentIds.Contains(d.Id.ToString())).Count()
            //}).ToListAsync();
            var query = await _context.DocumentTypes
       .Select(t => new DetailsDTO
       {
           Id = t.Id,
           Document_count = t.Documents.Count(d => documentIds.Contains(d.Id.ToString()))
       }).ToListAsync();
            return query;
        }

        public Task<PagedList<DocumentTypeDetailsDTO>> ListDocumentTypeDropDown<DocumentTypeDetailsDTO>(DocumentTypeParameters documenttypeparameters, string owner, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
