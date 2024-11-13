using Application.Dtos.CustomField;
using Application.Dtos.DocumentNote;
using Application.Dtos.Documents;
using Application.Dtos.Permission;
using Application.Exceptions;
using Application.Parameters;
using Application.Respository;
using Aspose.Pdf.Operators;
using DD.Core.Pagination;
using Domain.Documents;
using Elasticsearch.Net;
using Infrastructure.Persistence;
using Infrastructure.Serializers;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading;


namespace Infrastructure.Repositories
{
    public class DocumentsRepository : IDocumentRepository
    {
        private readonly DBContext _context;
        private readonly IElasticClient _elasticClient;
        public DocumentsRepository(DBContext context, IElasticClient elasticClient)
        {
            _context = context;
            _elasticClient = elasticClient;
        }
        public async Task AddAsync(Document entity, CancellationToken cancellationToken = default)
        {
            try
            {

                _context.Documents.Add(entity);

                await _context.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString());
            }
            //  return Task.FromResult(new Document());
        }

        public async Task IndexDocumentToElasticsearchAsync(Document document)
        {
            throw new NotImplementedException();
            //try
            //{
            //    // Serialize document to JSON, excluding the Tags property
            //    var jsonDocument = SerializeDocumentWithoutTags(document);

            //    // Index document in Elasticsearch
            //    var response = await _elasticClient.LowLevel.IndexAsync<StringResponse>("documents", PostData.String(jsonDocument));

            //    if (!response.Success)
            //    {
            //        // Handle indexing failure
            //        Console.WriteLine($"Failed to index document: {response.DebugInformation}");
            //    }
            //    else
            //    {
            //        // Indexing successful
            //        Console.WriteLine($"Document indexed successfully: {response.DebugInformation}");
            //    }
            //}
            //catch (Exception ex)
            //{
            //    // Handle indexing exception
            //    Console.WriteLine($"Error indexing document: {ex.Message}");
            //    throw; // Propagate the exception or handle as appropriate
            //}
        }

        public async Task AddRangeAsync(IEnumerable<Document> entities, CancellationToken cancellationToken = default)
        {
            try
            {
                await _context.Documents.AddRangeAsync(entities, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new DocumentsException($"An error occurred while adding the documents: {ex.Message}");
            }
        }
        public string SerializeDocumentWithoutTags(Document document)
        {
            JObject jsonObject = JObject.FromObject(document, new JsonSerializer
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
            jsonObject.Remove("Tags");
            return jsonObject.ToString(Formatting.None);
        }
        public async Task DeleteAsync(Expression<Func<Document, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var documentsToDelete = await _context.Documents.Where(predicate).ToListAsync(cancellationToken);

            foreach (var document in documentsToDelete)
            {
                _context.Documents.Remove(document);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Document entity, CancellationToken cancellationToken = default)
        {
            try
            {
                _context.Documents.Remove(entity);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new DocumentsException(ex.Message.ToString());
            }
        }

        public async Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var document = await _context.Documents.FindAsync(id, cancellationToken);

                if (document == null)
                {
                    throw new DocumentsException($"Document with ID {id} not found."); // Or a custom exception type
                }

                _context.Documents.Remove(document);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new DocumentsException(ex.Message.ToString());
            }
        }

        public async Task DeleteRangeAsync(IReadOnlyList<Document> entities, CancellationToken cancellationToken = default)
        {
            try
            {
                foreach (var entity in entities)
                {
                    _context.Documents.Remove(entity);
                }

                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new DocumentsException(ex.Message.ToString());
            }
        }

        public void Dispose()
        {
            _context?.Dispose();
        }

        public async Task<bool> ExistsAsync(Expression<Func<Document, bool>> predicate, CancellationToken cancellationToken = default)
        {
            // Check if any document satisfies the predicate
            bool documentExists = await _context.Documents.AnyAsync(predicate, cancellationToken);

            return documentExists;

        }

        public async Task<IReadOnlyList<Document>> FindAsync(Expression<Func<Document, bool>> predicate, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Documents.Where(predicate).ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new DocumentsException(ex.Message.ToString());
            }
        }

        public async Task<Document> FindByIdDetailsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            //try
            //{
            //    NewtonSoftService serializer = new NewtonSoftService();

            //    var query = await _context.Documents.Include(d => d.Tags).ThenInclude(d => d.Tag).Include(d => d.DocumentsCustomFields)
            //                               .Include(d => d.Notes).FirstOrDefaultAsync(d=>d.Id== id, cancellationToken);
            //    var tagIds = query.Tags.Select(t => t.TagId).ToList();
            //    List<DocumentNoteDto> notesDto = new List<DocumentNoteDto>();
            //    foreach (var note in query.Notes)
            //    {
            //        DocumentNoteDto noteDto = new DocumentNoteDto
            //        {
            //            Note = note.Note,
            //            Created = note.CreatedAt,
            //            User = note.User,
            //            Id = note.Id
            //        };
            //        notesDto.Add(noteDto);
            //    }



            //    var documentDto = new DocumentDetailsDTO
            //    {
            //        Id = query.Id,
            //        Title = query.Title,
            //        Content = query.Content,
            //        Owner = query.Owner,
            //        Archive_serial_number = query.Archive_Serial_Number,
            //        DocumentTypeId = query.DocumentTypeId,
            //        StoragePathId = query.StoragePathId,
            //        CorrespondentId = query.CorrespondentId,
            //        Tags = tagIds,
            //        Notes = notesDto,
            //        Custom_fields = query.DocumentsCustomFields
            //                                                    .Select(dc => new DocumentCustomFieldDTO
            //                                                    {
            //                                                        Field = dc.CustomFieldId,
            //                                                        Value = dc.Value
            //                                                    })
            //                                                    .ToList(),
            //        Permissions = new PermissionDto
            //        {
            //            View = new UserGroupPermission
            //            {
            //                Users = query.UsersView,
            //                Groups = query.GroupsView
            //            },
            //            Change = new UserGroupPermission
            //            {
            //                Users = query.UsersChange,
            //                Groups = query.GroupsChange
            //            }
            //        }
            //    };
            //    string serializedItems = serializer.Serialize(documentDto);
            //    DocumentDetailsDTO deserializedItems1 = serializer.Deserialize<DocumentDetailsDTO>(serializedItems);
            //    return deserializedItems1;

            //}
            //catch (Exception ex)
            //{
            //    throw new DocumentsException(ex.Message.ToString()); 
            //}
            try
            {
                // NewtonSoftService serializer = new NewtonSoftService();

                var query = await _context.Documents
                                        .Include(d => d.Tags)
                                            .ThenInclude(t => t.Tag)
                                        .Include(d => d.DocumentsCustomFields)
                                        .Include(d => d.Notes)
                                        .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

                //if (query == null)
                //{
                //    throw new DocumentsException("Document not found.");
                //}

                //var tagIds = query.Tags?.Select(t => t.TagId).ToList() ?? new List<Guid>();
                //List<DocumentNoteDto> notesDto = new List<DocumentNoteDto>();

                //if (query.Notes != null)
                //{
                //    foreach (var note in query.Notes)
                //    {
                //        DocumentNoteDto noteDto = new DocumentNoteDto
                //        {
                //            Note = note.Note,
                //            Created = note.CreatedAt,
                //            User = note.User,
                //            Id = note.Id
                //        };
                //        notesDto.Add(noteDto);
                //    }
                //}

                //var documentDto = new DocumentDetailsDTO
                //{
                //    Id = query.Id,
                //    Title = query.Title,
                //    Content = query.Content,
                //    Owner = query.Owner,
                //    FileData= query.FileData,
                //    Base64Data= query.Base64Data,
                //    MimeType = query.MimeType,
                //    ArchiveSerialNumber = query.Archive_Serial_Number,
                //    DocumentTypeId = query.DocumentTypeId,
                //    StoragePathId = query.StoragePathId,
                //    CorrespondentId = query.CorrespondentId,
                //    Tags = tagIds,
                //    Notes = notesDto,
                //    Custom_fields = query.DocumentsCustomFields?.Select(dc => new DocumentCustomFieldDTO
                //    {
                //        Field = dc.CustomFieldId,
                //        Value = dc.Value
                //    }).ToList() ?? new List<DocumentCustomFieldDTO>(),
                //    Permissions = new PermissionDto
                //    {
                //        View = new UserGroupPermission
                //        {
                //            Users = query.UsersView ?? new List<string>(),
                //            Groups = query.GroupsView ?? new List<string>()
                //        },
                //        Change = new UserGroupPermission
                //        {
                //            Users = query.UsersChange ?? new List<string>(),
                //            Groups = query.GroupsChange ?? new List<string>()
                //        }
                //    }
                //};

                //string serializedItems = serializer.Serialize(documentDto);
                //DocumentDetailsDTO deserializedItems1 = serializer.Deserialize<DocumentDetailsDTO>(serializedItems);
                return query;
            }
            catch (Exception ex)
            {
                throw new DocumentsException(ex.Message);
            }

        }

        public Task<Document?> FindOneAsync(Expression<Func<Document, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<IReadOnlyList<Document>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var documents = await _context.Documents.Include(d => d.Tags).ThenInclude(d => d.Tag).ToListAsync(cancellationToken).ConfigureAwait(false);

            return documents;
        }

        public async Task<PagedList<DocumentDetailsDTO>> GetPagedDocumentAsync<DocumentDetailsDTO>(DocumentParameters documentparameters, CancellationToken cancellationToken = default)
        {
            List<Guid> guids = new List<Guid>();

            NewtonSoftService serializer = new NewtonSoftService();

            // Get the documents from the database
            var query = _context.Documents
                                          .Include(d => d.DocumentsCustomFields).ThenInclude(dc => dc.CustomField)
                                           .Include(d => d.Tags)
                                           .ThenInclude(d => d.Tag)

                                          .Include(dt => dt.Correspondent)
                                          .Include(d => d.Document_Type)
                                          .Include(d => d.Notes)
                                          .AsQueryable();


            foreach (var document in query)
            {
                Console.WriteLine(document.Created);
                guids.Add(document.Id);
            }
            // Count the total number of documents
            int count = await query.CountAsync();

            // Get the paged documents
            var items = await query.Skip(documentparameters.PageSize * (documentparameters.Page - 1))
                                   .Take(documentparameters.PageSize)
                                   .ToListAsync();

            // Map documents to DocumentDetailsDTO using Mapster
            var mappedItems = items.Adapt<List<DocumentDetailsDTO>>();
            // Serialize the paged documents to a JSON string
            //string serializedItems = serializer.Serialize(items);

            //// Deserialize the JSON string to a list of documents

            //List<DocumentDetailsDTO> deserializedlist = serializer.Deserialize<List<DocumentDetailsDTO>>(serializedItems);


            // Create a new PagedList with the deserialized items
            return new PagedList<DocumentDetailsDTO>(mappedItems, count, documentparameters.Page, documentparameters.PageSize, guids);
        }



        public async Task UpdateAsync(Document entity, CancellationToken cancellationToken = default)
        {
            try
            {
                var existingEntity = await _context.Documents.FindAsync(entity.Id);

                if (existingEntity == null)
                {
                    throw new DocumentsException($"Document with ID {entity.Id} not found.");
                }

                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new DocumentsException(ex.Message.ToString());
            }
        }
        public async Task<DocumentSuggestionsDto> FindByIdDetailsSuggestionsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                NewtonSoftService serializer = new NewtonSoftService();

                var query = await _context.Documents.Include(d => d.Tags).ThenInclude(d => d.Tag)
                                           .Include(d => d.Notes).FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
                string serializedItems = serializer.Serialize(query);
                Console.WriteLine(serializedItems);
                DocumentSuggestionsDto deserializedItems1 = serializer.Deserialize<DocumentSuggestionsDto>(serializedItems);
                return deserializedItems1;

            }
            catch (Exception ex)
            {
                throw new DocumentsException(ex.Message.ToString());
            }
        }

        public async Task<Domain.Documents.Document?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Documents.Include(d => d.Tags).ThenInclude(t => t.Tag)
                                                 .Include(d => d.DocumentsCustomFields).Include(d => d.Document_Type)
                                                  .Include(d => d.StoragePath).Include(d => d.Correspondent)// Eagerly load DocumentsCustomFields
                                                 .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new TagException(ex.Message.ToString());
            }
        }

        public async Task<PagedList<DocumentDetailsDTO>> GetDocumentsByTagID<DocumentDetailsDTO>(DocumentParameters documentparameters, List<Guid>? Id, CancellationToken cancellationToken = default)
        {
            List<Guid> guids = new List<Guid>();

            NewtonSoftService serializer = new NewtonSoftService();

            // Get the documents from the database
            var query = _context.Documents
                                          .Include(d => d.DocumentsCustomFields).ThenInclude(dc => dc.CustomField)
                                           .Include(d => d.Tags)
                                           .ThenInclude(d => d.Tag)

                                          .Include(dt => dt.Correspondent)
                                          .Include(d => d.Document_Type)
                                          .Include(d => d.Notes)/*.Where(d => d.Tags.Any(dt=> Id.Contains(dt.TagId)))*/
                                          .Where(d => Id.All(guid => d.Tags.Any(dt => dt.TagId == guid)))
                                          .AsQueryable();


            foreach (var document in query)
            {
                Console.WriteLine(document.Created);
                guids.Add(document.Id);
            }
            // Count the total number of documents
            int count = await query.CountAsync();

            // Get the paged documents
            var items = await query.Skip(documentparameters.PageSize * (documentparameters.Page - 1))
                                   .Take(documentparameters.PageSize)
                                   .ToListAsync();

            // Map documents to DocumentDetailsDTO using Mapster
            var mappedItems = items.Adapt<List<DocumentDetailsDTO>>();



            // Create a new PagedList with the deserialized items
            return new PagedList<DocumentDetailsDTO>(mappedItems, count, documentparameters.Page, documentparameters.PageSize, guids);

        }

        public async Task<PagedList<DocumentDetailsDTO>> GetDocumentsByCorrespondentID<DocumentDetailsDTO>(DocumentParameters documentparameters, List<Guid>? Id, CancellationToken cancellationToken = default)
        {
            List<Guid> guids = new List<Guid>();

            NewtonSoftService serializer = new NewtonSoftService();

            // Get the documents from the database
            var query = _context.Documents
                                          .Include(d => d.DocumentsCustomFields).ThenInclude(dc => dc.CustomField)
                                          .Include(d => d.Tags)
                                          .ThenInclude(d => d.Tag)
                                          .Include(dt => dt.Correspondent)
                                          .Include(d => d.Document_Type)
                                          .Include(d => d.Notes)
                                          .Where(d => d.CorrespondentId.HasValue && Id.Contains(d.CorrespondentId.Value))
                                          .AsQueryable();


            foreach (var document in query)
            {
                Console.WriteLine(document.Created);
                guids.Add(document.Id);
            }
            // Count the total number of documents
            int count = await query.CountAsync();

            // Get the paged documents
            var items = await query.Skip(documentparameters.PageSize * (documentparameters.Page - 1))
                                   .Take(documentparameters.PageSize)
                                   .ToListAsync();

            // Map documents to DocumentDetailsDTO using Mapster
            var mappedItems = items.Adapt<List<DocumentDetailsDTO>>();



            // Create a new PagedList with the deserialized items
            return new PagedList<DocumentDetailsDTO>(mappedItems, count, documentparameters.Page, documentparameters.PageSize, guids);


        }

        public async Task<PagedList<DocumentDetailsDTO>> GetDocumentsByDocumentTypeID<DocumentDetailsDTO>(DocumentParameters documentparameters, List<Guid>? Id, CancellationToken cancellationToken = default)
        {
            List<Guid> guids = new List<Guid>();

            NewtonSoftService serializer = new NewtonSoftService();

            // Get the documents from the database
            var query = _context.Documents
                                          .Include(d => d.DocumentsCustomFields).ThenInclude(dc => dc.CustomField)
                                          .Include(d => d.Tags)
                                          .ThenInclude(d => d.Tag)
                                          .Include(dt => dt.Correspondent)
                                          .Include(d => d.Document_Type)
                                          .Include(d => d.Notes).Where(d => d.DocumentTypeId.HasValue && Id.Contains(d.DocumentTypeId.Value))
                                          .AsQueryable();


            foreach (var document in query)
            {
                Console.WriteLine(document.Created);
                guids.Add(document.Id);
            }
            // Count the total number of documents
            int count = await query.CountAsync();

            // Get the paged documents
            var items = await query.Skip(documentparameters.PageSize * (documentparameters.Page - 1))
                                   .Take(documentparameters.PageSize)
                                   .ToListAsync();

            // Map documents to DocumentDetailsDTO using Mapster
            var mappedItems = items.Adapt<List<DocumentDetailsDTO>>();



            // Create a new PagedList with the deserialized items
            return new PagedList<DocumentDetailsDTO>(mappedItems, count, documentparameters.Page, documentparameters.PageSize, guids);

        }
        public async Task<PagedList<DocumentDetailsDTO>> GetDocumentByTagCorrespondentDocumentType<DocumentDetailsDTO>(
                        DocumentSearchDto documentSearch,
                        Guid groupId,
                        CancellationToken cancellationToken = default)
        {
            try
            {
                var query = InitializeBaseQuery(groupId);

                query = ApplyFilters(query, documentSearch);
                query = ApplyOrdering(query, documentSearch.Ordering);

                // Count the total number of documents after filters
                int count = await query.CountAsync(cancellationToken);

                // Get paged and mapped documents
                var pagedDocuments = await GetPagedDocuments(query, documentSearch, cancellationToken);
                var mappedItems = pagedDocuments.Adapt<List<DocumentDetailsDTO>>();

                var guids = pagedDocuments.Select(doc => doc.Id).ToList();

                return new PagedList<DocumentDetailsDTO>(mappedItems, count,
                    documentSearch.DocumentParameters.Page,
                    documentSearch.DocumentParameters.PageSize, guids);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        private IQueryable<Document> InitializeBaseQuery(Guid groupId)
        {
            return _context.Documents
                .Include(d => d.DocumentsCustomFields).ThenInclude(dc => dc.CustomField)
                .Include(d => d.Tags).ThenInclude(d => d.Tag)
                .Include(d => d.Correspondent)
                .Include(d => d.Document_Type)
                .Include(d => d.Notes)
                .Where(d => d.GroupId == groupId)
                .AsQueryable();
        }

        private IQueryable<Document> ApplyFilters(IQueryable<Document> query, DocumentSearchDto documentSearch)
        {
            try
            {
                // Filter by Document Type
                if (documentSearch.DocumentTypeID != null && documentSearch.DocumentTypeID.Any())
                {
                    query = query.Where(d => d.DocumentTypeId.HasValue && documentSearch.DocumentTypeID.Contains(d.DocumentTypeId.Value));
                }

                // Filter by Correspondent
                if (documentSearch.CorrespondentID != null && documentSearch.CorrespondentID.Any())
                {
                    query = query.Where(d => d.CorrespondentId.HasValue && documentSearch.CorrespondentID.Contains(d.CorrespondentId.Value));
                }

                // Filter by Tags
                if (documentSearch.TagID != null && documentSearch.TagID.Any())
                {
                    query = query.Where(d => documentSearch.TagID.All(guid => d.Tags.Any(dt => dt.TagId == guid)));
                }

                // Filter by Title
                if (!string.IsNullOrEmpty(documentSearch.TitleContains))
                {
                    query = query.Where(d => d.Title.ToLower().Contains(documentSearch.TitleContains.ToLower()));
                }
                if (!string.IsNullOrEmpty(documentSearch.TitleContent))
                {
                    string titleContentLower = documentSearch.TitleContent.ToLower();
                    query = query.Where(d => d.Content.ToLower().Contains(titleContentLower) || d.Title.ToLower().Contains(titleContentLower));
                }
                // Filter by Storage Path
                if (documentSearch.StoragePathID != null && documentSearch.StoragePathID.Any())
                {
                    query = query.Where(d => d.StoragePathId.HasValue && documentSearch.StoragePathID.Contains(d.StoragePathId.Value));
                }

                // Filter by Created Date Range
                if (documentSearch.CreatedFrom.HasValue)
                {
                    query = query.Where(d => d.CreatedOn >= documentSearch.CreatedFrom.Value);
                }
                if (documentSearch.CreatedTo.HasValue)
                {
                    query = query.Where(d => d.CreatedOn <= documentSearch.CreatedTo.Value);
                }

                // Filter by Owner
                query = ApplyOwnerFilters(query, documentSearch);

                // Filter by Archive Serial Number
                query = ApplyArchiveSerialNumberFilters(query, documentSearch);

                // General Search
                //if (!string.IsNullOrEmpty(documentSearch.Search))
                //{
                //    string pattern = @"ASN(\d+)";


                //    query = query.Where(d => (d.Owner != null && d.Owner == documentSearch.Search) ||
                //          (d.Content != null && d.Content == documentSearch.Search) ||
                //          (!string.IsNullOrEmpty(d.Correspondent.Name) && d.Correspondent.Name.Contains(documentSearch.Search)) ||
                //          (d.Tags != null && d.Tags.Any(t => !string.IsNullOrEmpty(t.Tag.Name) && t.Tag.Name.Contains(documentSearch.Search))) ||
                //          (!string.IsNullOrEmpty(d.Document_Type.Name) && d.Document_Type.Name.Contains(documentSearch.Search)) ||
                //          (!string.IsNullOrEmpty(d.StoragePath.Name) && d.StoragePath.Name.Contains(documentSearch.Search)) ||
                //          (d.DocumentsCustomFields != null && d.DocumentsCustomFields.Any(c => !string.IsNullOrEmpty(c.CustomField.Name) && c.CustomField.Name.Contains(documentSearch.Search))) 
                //         );
                //}

                return query;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        private IQueryable<Document> ApplyOwnerFilters(IQueryable<Document> query, DocumentSearchDto documentSearch)
        {
            if (!string.IsNullOrEmpty(documentSearch.Owner) && documentSearch.OwnerIsNull != 1)
            {
                //var q1 = query.ToList();
                //query = query.Where(d =>
                //                        //  d.Owner.ToLower().Contains(documentSearch.Owner.ToLower()) ||
                //                        d.UsersView.Any(u => u.Contains(documentSearch.Owner))
                //                         //||
                //                         //d.UsersChange.Any(u => u.Contains(documentSearch.Owner)) ||
                //                         //d.GroupsView.Any(g => documentSearch.Groups.Contains(g)) ||
                //                         //d.GroupsChange.Any(g => documentSearch.Groups.Contains(g))
                //                         );
                //var q=query.ToList();
            }

            if (!string.IsNullOrEmpty(documentSearch.OwnerId))
            {
                query = query.Where(d => d.Owner.ToLower().Contains(documentSearch.OwnerId.ToLower()));
            }

            if (!string.IsNullOrEmpty(documentSearch.OwnerIdNone))
            {
                query = query.Where(d => d.UsersView.Any(u => u.Contains(documentSearch.Owner)));
            }

            if (documentSearch.OwnerIsNull == 1)
            {
                query = query.Where(d => d.Owner == null || d.Owner == "");
            }

            return query;
        }

        private IQueryable<Document> ApplyArchiveSerialNumberFilters(IQueryable<Document> query, DocumentSearchDto documentSearch)
        {
            if (documentSearch.ArchiveSerialNumber.HasValue)
            {
                query = FilterByArchiveSerialNumber(query, documentSearch.ArchiveSerialNumber.Value);
            }

            if (documentSearch.ArchiveSerialNumberIsNull.HasValue)
            {
                if (documentSearch.ArchiveSerialNumberIsNull.Value == 1)
                {
                    query = query.Where(d => string.IsNullOrEmpty(d.Archive_Serial_Number));
                }
                else
                {
                    query = query.Where(d => !string.IsNullOrEmpty(d.Archive_Serial_Number));
                }
            }

            if (documentSearch.ArchiveSerialNumberGT.HasValue)
            {
                query = FilterByArchiveSerialNumberComparison(query, documentSearch.ArchiveSerialNumberGT.Value, greaterThan: true);
            }

            if (documentSearch.ArchiveSerialNumberLT.HasValue)
            {
                query = FilterByArchiveSerialNumberComparison(query, documentSearch.ArchiveSerialNumberLT.Value, greaterThan: false);
            }

            return query;
        }

        private IQueryable<Document> ApplyOrdering(IQueryable<Document> query, string ordering)
        {
            if (string.IsNullOrEmpty(ordering)) return query;

            return ordering switch
            {
                "archive_serial_number" => query.OrderBy(d => d.Archive_Serial_Number),
                "-archive_serial_number" => query.OrderByDescending(d => d.Archive_Serial_Number),
                "correspondent__name" => query.OrderBy(d => d.Correspondent.Name),
                "-correspondent__name" => query.OrderByDescending(d => d.Correspondent.Name),
                "created" => query.OrderBy(d => d.Created),
                "-created" => query.OrderByDescending(d => d.Created),
                "title" => query.OrderBy(d => d.Title),
                "-title" => query.OrderByDescending(d => d.Title),
                "document_type__name" => query.OrderBy(d => d.Document_Type.Name),
                "-document_type__name" => query.OrderByDescending(d => d.Document_Type.Name),
                _ => query
            };
        }

        private async Task<List<Document>> GetPagedDocuments(IQueryable<Document> query, DocumentSearchDto documentSearch, CancellationToken cancellationToken)
        {
            return await query.OrderByDescending(d => d.CreatedOn)
                              .Skip(documentSearch.DocumentParameters.PageSize * (documentSearch.DocumentParameters.Page - 1))
                              .Take(documentSearch.DocumentParameters.PageSize)
                              .ToListAsync(cancellationToken);
        }

        private IQueryable<Document> FilterByArchiveSerialNumber(IQueryable<Document> query, int serialNumber)
        {
            string pattern = @"ASN(\d+)";
            return query.Where(d => Regex.Match(d.Archive_Serial_Number, pattern).Groups[1].Value == serialNumber.ToString());
        }
        private IQueryable<Document> FilterByArchiveSerialNumberComparison(IQueryable<Document> query, int serialNumber, bool greaterThan)
        {
            string pattern = @"ASN(\d+)";

            // Use a helper method for the logic and call it in the Where clause
            return query.Where(d => IsArchiveSerialNumberValid(d.Archive_Serial_Number, pattern, serialNumber, greaterThan));
        }

        private bool IsArchiveSerialNumberValid(string archiveSerialNumber, string pattern, int serialNumber, bool greaterThan)
        {
            var match = Regex.Match(archiveSerialNumber, pattern);
            if (match.Success && int.TryParse(match.Groups[1].Value, out var num))
            {
                return greaterThan ? num > serialNumber : num < serialNumber;
            }
            return false;
        }
        //public async Task<PagedList<DocumentDetailsDTO>> GetDocumentByTagCorrespondentDocumentType<DocumentDetailsDTO>(
        //                                                                                                                DocumentSearchDto documentSearch,
        //                                                                                                                Guid groupId,
        //                                                                                                                CancellationToken cancellationToken = default)
        //{
        //    try
        //    {


        //        List<Guid> guids = new List<Guid>();

        //        // Start with the base query
        //        var query = _context.Documents
        //                            .Include(d => d.DocumentsCustomFields).ThenInclude(dc => dc.CustomField)
        //                            .Include(d => d.Tags).ThenInclude(d => d.Tag)
        //                            .Include(d => d.Correspondent)
        //                            .Include(d => d.Document_Type)
        //                            .Include(d => d.Notes).Where(d => d.GroupId == groupId)
        //                            .AsQueryable();

        //        // Apply filters only if the lists are not null
        //        if (documentSearch.DocumentTypeID != null && documentSearch.DocumentTypeID.Any())
        //        {
        //            query = query.Where(d => d.DocumentTypeId.HasValue && documentSearch.DocumentTypeID.Contains(d.DocumentTypeId.Value));
        //        }

        //        if (documentSearch.CorrespondentID != null && documentSearch.CorrespondentID.Any())
        //        {
        //            query = query.Where(d => d.CorrespondentId.HasValue && documentSearch.CorrespondentID.Contains(d.CorrespondentId.Value));
        //        }

        //        if (documentSearch.TagID != null && documentSearch.TagID.Any())
        //        {
        //            query = query.Where(d => documentSearch.TagID.All(guid => d.Tags.Any(dt => dt.TagId == guid)));
        //        }
        //        if (!string.IsNullOrEmpty(documentSearch.TitleContains))
        //        {
        //            query = query.Where(d => d.Title.ToLower().Contains(documentSearch.TitleContains.ToLower()));

        //        }
        //        if (documentSearch.StoragePathID != null && documentSearch.StoragePathID.Any())
        //        {
        //            query = query.Where(d => d.StoragePathId.HasValue && documentSearch.StoragePathID.Contains(d.StoragePathId.Value));

        //        }
        //        if (documentSearch.CreatedFrom.HasValue)
        //        {
        //            query = query.Where(d => d.CreatedOn >= documentSearch.CreatedFrom.Value);
        //        }

        //        if (documentSearch.CreatedTo.HasValue)
        //        {
        //            query = query.Where(d => d.CreatedOn <= documentSearch.CreatedTo.Value);
        //        }
        //        if (!string.IsNullOrEmpty(documentSearch.Owner) && documentSearch.OwnerIsNull != 1)
        //        {
        //            query = query.Where(d=>d.Owner.ToLower().Contains(documentSearch.Owner.ToLower()) ||  d.UsersView.Any(d=>d.Contains(documentSearch.Owner))  || d.UsersChange.Any(d => d.Contains(documentSearch.Owner)) || d.GroupsView.Any(group => documentSearch.Groups.Contains(group)) || d.GroupsChange.Any(group => documentSearch.Groups.Contains(group))) ;
        //        }
        //        if (!string.IsNullOrEmpty(documentSearch.OwnerId))
        //        {
        //            query = query.Where(d => d.Owner.ToLower().Contains(documentSearch.OwnerId.ToLower()));
        //        }
        //        if (!string.IsNullOrEmpty(documentSearch.OwnerIdNone))
        //        {
        //            query = query.Where(d => d.UsersView.Any(d => d.Contains(documentSearch.Owner)));
        //        }
        //        if (documentSearch.OwnerIsNull == 1)
        //        {
        //            query = query.Where(d => d.Owner == null || d.Owner == "");
        //        }
        //        if (!string.IsNullOrEmpty(documentSearch.Search))
        //        {
        //            query = query.Where(d => d.Owner == documentSearch.Search || d.Content == documentSearch.Search || d.Correspondent.Name.Contains(documentSearch.Search) || d.Tags.Any(t => t.Tag.Name.Contains(documentSearch.Search)) || d.Document_Type.Name.Contains(documentSearch.Search) || d.StoragePath.Name.Contains(documentSearch.Search) || d.DocumentsCustomFields.Any(c => c.CustomField.Name.Contains(documentSearch.Search)));
        //        }
        //        if (documentSearch.ArchiveSerialNumber.HasValue)
        //        {
        //            string pattern = @"ASN(\d+)";
        //            var matchingDocs = query.Where(d => !string.IsNullOrEmpty(d.Archive_Serial_Number)).ToList();
        //            var filteredDocs = matchingDocs.Where(d =>
        //            {
        //                if (int.TryParse(Regex.Match(d.Archive_Serial_Number, pattern).Groups[1].Value, out var num))
        //                {
        //                    return num == documentSearch.ArchiveSerialNumber.Value;
        //                }
        //                return false;
        //            });

        //            query = query.Where(d => filteredDocs.Contains(d));
        //        }
        //        if (documentSearch.ArchiveSerialNumberIsNull.HasValue)
        //        {
        //            if (documentSearch?.ArchiveSerialNumberIsNull.Value == 1)
        //            {
        //                query = query.Where(d => string.IsNullOrEmpty(d.Archive_Serial_Number));
        //            }
        //            else if (documentSearch?.ArchiveSerialNumberIsNull.Value == 0)
        //            {
        //                query = query.Where(d => !string.IsNullOrEmpty(d.Archive_Serial_Number));
        //            }
        //        }

        //        if (documentSearch.ArchiveSerialNumberGT.HasValue)
        //        {
        //            string pattern = @"ASN(\d+)";
        //            // Fetch documents where Archive_Serial_Number is not null or empty
        //            var matchingDocs = query.Where(d => !string.IsNullOrEmpty(d.Archive_Serial_Number)).ToList();

        //            // Filter documents based on the comparison with archive_serial_number__gt
        //            var filteredDocs = matchingDocs.Where(d =>
        //            {
        //                var match = Regex.Match(d.Archive_Serial_Number, pattern);
        //                if (match.Success && int.TryParse(match.Groups[1].Value, out var num))
        //                {
        //                    return num > documentSearch.ArchiveSerialNumberGT.Value;
        //                }
        //                return false;
        //            });

        //            // Update the query to include only filtered documents
        //            query = query.Where(d => filteredDocs.Contains(d));
        //        }
        //        if (documentSearch.ArchiveSerialNumberLT.HasValue)
        //        {
        //            string pattern = @"ASN(\d+)";

        //            // Fetch documents where Archive_Serial_Number is not null or empty
        //            var matchingDocs = query.Where(d => !string.IsNullOrEmpty(d.Archive_Serial_Number)).ToList();

        //            // Filter documents based on the comparison with archive_serial_number__lt
        //            var filteredDocs = matchingDocs.Where(d =>
        //            {
        //                var match = Regex.Match(d.Archive_Serial_Number, pattern);
        //                if (match.Success && int.TryParse(match.Groups[1].Value, out var num))
        //                {
        //                    // Ensure archive_serial_number__lt has a value before accessing .Value
        //                    return documentSearch.ArchiveSerialNumberLT.HasValue && num < documentSearch.ArchiveSerialNumberLT.Value;
        //                }
        //                return false;
        //            });

        //            // Update the query to include only filtered documents
        //            query = query.Where(d => filteredDocs.Contains(d));
        //        }
        //        if (!string.IsNullOrEmpty(documentSearch.Ordering))
        //        {
        //            if (documentSearch.Ordering == "archive_serial_number")
        //            {
        //                query = query.OrderBy(d => d.Archive_Serial_Number);
        //            }
        //            else if (documentSearch.Ordering == "-archive_serial_number")
        //            {
        //                query = query.OrderByDescending(d => d.Archive_Serial_Number);
        //            }
        //            else if (documentSearch.Ordering == "correspondent__name")
        //            {
        //                query = query.OrderBy(d => d.Correspondent.Name);
        //            }
        //            else if (documentSearch.Ordering == "-correspondent__name")
        //            {
        //                query = query.OrderByDescending(d => d.Correspondent.Name);
        //            }
        //            else if (documentSearch.Ordering == "created")
        //            {
        //                query = query.OrderBy(d => d.Created);
        //            }
        //            else if (documentSearch.Ordering == "-created")
        //            {
        //                query = query.OrderByDescending(d => d.Created);
        //            }
        //            else if (documentSearch.Ordering == "title")
        //            {
        //                query = query.OrderBy(d => d.Title);
        //            }
        //            else if (documentSearch.Ordering == "-title")
        //            {
        //                query = query.OrderByDescending(d => d.Title);
        //            }
        //            else if (documentSearch.Ordering == "document_type__name")
        //            {
        //                query = query.OrderBy(d => d.Document_Type.Name);
        //            }
        //            else if (documentSearch.Ordering == "-document_type__name")
        //            {
        //                query = query.OrderByDescending(d => d.Document_Type.Name);
        //            }

        //        }
        //        // Count the total number of documents
        //        int count = await query.CountAsync(cancellationToken);


        //        // Get the paged documents
        //        var items = await query.OrderByDescending(d => d.CreatedOn).Skip(documentSearch.DocumentParameters.PageSize * (documentSearch.DocumentParameters.Page - 1))
        //                               .Take(documentSearch.DocumentParameters.PageSize)
        //                               .ToListAsync(cancellationToken);

        //        // Map documents to DocumentDetailsDTO using Mapster
        //        var mappedItems = items.Adapt<List<DocumentDetailsDTO>>();

        //        // Collect document IDs
        //        guids.AddRange(items.Select(doc => doc.Id));

        //        // Create a new PagedList with the mapped items
        //        return new PagedList<DocumentDetailsDTO>(mappedItems, count, documentSearch.DocumentParameters.Page, documentSearch.DocumentParameters.PageSize, guids);
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error(ex.Message);
        //        throw new Exception(ex.Message);
        //    }
        //}

        public async Task<List<Document>> GetAllDocumentByOwner(string owner)
        {
            var documents = await _context.Documents.Include(d => d.Tags).ThenInclude(d => d.Tag).Where(d => d.Owner == owner).ToListAsync();
            return documents;
        }
        public async Task<List<Document>> GetAllDocuments()
        {
            try
            {
                var documents = await _context.Documents.Include(d => d.Document_Type).ToListAsync();
                return documents;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception(ex.Message);
            }
        }
        public async Task<List<Document>> GetAllDocumentByGroup(Guid groupId)
        {
            try
            {
                var documents = await _context.Documents.Include(d => d.Tags).ThenInclude(d => d.Tag).Include(d => d.Document_Type).Where(d => d.GroupId == groupId).ToListAsync();
                return documents;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        private static int? ExtractArchiveSerialNumber(string archiveSerialNumber)
        {
            if (string.IsNullOrEmpty(archiveSerialNumber)) return null;

            string pattern = @"ASN(\d+)";
            var match = Regex.Match(archiveSerialNumber, pattern);

            if (match.Success && int.TryParse(match.Groups[1].Value, out int result))
            {
                return result;
            }

            return null;
        }
        public async Task<Document> GetDocumentByGroupIdAndASN(Guid groupId, string baseWordASN)
        {
            try
            {
                
                return await _context.Documents
                    .Where(d => d.GroupId == groupId && d.Archive_Serial_Number.StartsWith(baseWordASN))
                    .OrderByDescending(d => d.CreatedOn)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
         
                Log.Error(ex, "Error retrieving document by GroupId and baseWordASN");
                throw;
            }
        }
    }

}