using Application.Dtos.Documents;
using Application.Dtos.Templates;
using Application.Exceptions;
using Application.Parameters;
using Application.Respository;
using DD.Core.Pagination;
using Domain.Templates;
using Infrastructure.Persistence;
using Infrastructure.Serializers;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Threading;


namespace Infrastructure.Repositories
{
    public class TemplateRepository : ITemplateRepository
    {
        private readonly DBContext _context;

        public TemplateRepository(DBContext context)
        {
            _context = context;
        }
        public Task AddAsync(Template entity, CancellationToken cancellationToken = default)
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
            return Task.FromResult(new Template());
        }

        public Task AddRangeAsync(IEnumerable<Template> entities, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(Expression<Func<Template, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(Template entity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var templates = await _context.Templates.FindAsync(id, cancellationToken);

                if (templates == null)
                {
                    throw new TemplateException($"Template with ID {id} not found."); // Or a custom exception type
                }

                _context.Templates.Remove(templates);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new TemplateException(ex.Message.ToString());
            }
        }

        public Task DeleteRangeAsync(IReadOnlyList<Template> entities, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _context?.Dispose();
        }

        public async Task<bool> ExistsAsync(Expression<Func<Template, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _context.Templates.AnyAsync(predicate, cancellationToken);
        }

        public Task<IReadOnlyList<Template>> FindAsync(Expression<Func<Template, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<Template?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Templates.FindAsync(id, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new TemplateException(ex.Message.ToString());
            }
        }

        public Task<Template?> FindOneAsync(Expression<Func<Template, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<IReadOnlyList<Template>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var templates = await _context.Templates.ToListAsync(cancellationToken).ConfigureAwait(false);

            return templates;
        }

        public async Task<List<Template>> GetAllByOwner(string id)
        {
            try
            {

                    var templates = await _context.Templates.Where(d=>d.Owner ==id ).ToListAsync();

                    return templates;
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<Template>> GetAllByOrderAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var templates = await _context.Templates
                    .OrderBy(rule => (int)rule.Order)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);

                return templates;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving Temlplates: {ex.Message}");
                throw; // Re-throw the exception after logging
            }
        }

        
        public async Task<PagedList<PagedTemplate>> GetPagedTemplatesAsync(TemplateParametres templateparameters, string owner ,CancellationToken cancellationToken = default)
        {
            List<Guid> guids = new List<Guid>();

            // Fetch templates from the context and count the total number of templates
            var templates = await _context.Templates.Where(t=>t.Owner == owner || t.Assign_view_users.Contains(owner))
                                          .Skip(templateparameters.PageSize * (templateparameters.Page - 1))
                                          .Take(templateparameters.PageSize)
                                          .ToListAsync(cancellationToken);

            var totalTemplates = await _context.Templates.CountAsync(cancellationToken);

            // Convert templates to PagedTemplate objects
            var pagedTemplates = templates.Select(template => new PagedTemplate
            {
                Id = template.Id,
                Name = template.Name,
                Order = template.Order,
                DocumentClassification = template.DocumentClassification,
                Sources = template.Sources,
                Filter_filename = template.FilterFilename,
                Filter_path = template.FilterPath,
                Filter_mailrule = template.FilterMailrule,
                Assign_title = template.AssignTitle,
                Assign_tags = template.AssignTags,
                Assign_document_type = template.AssignDocumentType,
                Assign_correspondent = template.AssignCorrespondent,
                Assign_storage_path = template.AssignStoragePath,
                Type = template.Type,
                Matching_algorithm = template.Content_matching_algorithm,
                Match = template.Content_matching_pattern,
                Filter_has_tags = template.Has_Tags,
                Filter_has_correspondent = template.Has_Correspondent,
                Filter_has_document_type = template.Has_Document_Type,
                Is_insensitive = template.Is_Insensitive,
                Is_Enabled=template.Is_Enabled
            }).ToList();

            // Collect document IDs
            guids.AddRange(pagedTemplates.Select(template => template.Id));

            // Return the paged list
            return new PagedList<PagedTemplate>(pagedTemplates, totalTemplates, templateparameters.Page, templateparameters.PageSize, guids);
        }

        
            public async Task UpdateAsync(Template entity, CancellationToken cancellationToken = default)
            {
                if (entity == null)
                {
                    throw new ArgumentNullException(nameof(entity));
                }

                // Assuming you have a DbContext (e.g., _context) and a DbSet<Template> (e.g., _context.Templates)
                var existingEntity = await _context.Templates.FindAsync(new object[] { entity.Id }, cancellationToken);

                if (existingEntity == null)
                {
                    throw new KeyNotFoundException($"Entity with id {entity.Id} not found.");
                }

                // Update properties
                existingEntity.Name = entity.Name;
                existingEntity.Order = entity.Order;
                existingEntity.Sources = entity.Sources;
                existingEntity.FilterFilename = entity.FilterFilename;
                existingEntity.Owner = entity.Owner;
                existingEntity.FilterPath = entity.FilterPath;
                existingEntity.FilterMailrule = entity.FilterMailrule;
                existingEntity.AssignTitle = entity.AssignTitle;
                existingEntity.AssignTags = entity.AssignTags;
                existingEntity.AssignDocumentType = entity.AssignDocumentType;
                existingEntity.AssignCorrespondent = entity.AssignCorrespondent;
                existingEntity.AssignStoragePath = entity.AssignStoragePath;
                existingEntity.Type = entity.Type;
                existingEntity.Content_matching_algorithm = entity.Content_matching_algorithm;
                existingEntity.Content_matching_pattern = entity.Content_matching_pattern;
                existingEntity.DocumentClassification = entity.DocumentClassification;
                existingEntity.Has_Tags = entity.Has_Tags;
                existingEntity.Has_Correspondent = entity.Has_Correspondent;
                existingEntity.Has_Document_Type = entity.Has_Document_Type;
                existingEntity.Is_Insensitive = entity.Is_Insensitive;

                // Mark the entity as modified
                _context.Entry(existingEntity).State = EntityState.Modified;

                // Save changes asynchronously
                await _context.SaveChangesAsync(cancellationToken);
            }

        
    }
}
