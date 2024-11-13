using Application.Dtos.CustomField;
using Application.Dtos.Tag;
using Application.Exceptions;
using Application.Parameters;
using Application.Respository;
using DD.Core.Pagination;
using Domain.DocumentManagement.CustomFields;
using Infrastructure.Persistence;
using Infrastructure.Serializers;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Linq;
using System.Linq.Expressions;


namespace Infrastructure.Repositories
{
    public class CustomfieldRepository : ICustomFieldRepository
    {
        private readonly DBContext _context;

        public CustomfieldRepository(DBContext context)
        {
            _context = context;
        }
        public async Task AddAsync(CustomField entity, CancellationToken cancellationToken = default)
        {
            try
            {
           
                _context.CustomFields.Add(entity);
                await _context.SaveChangesAsync(cancellationToken);
            
              
            }
            catch (Exception ex)
            {
                throw new CustomFieldException($"An error occurred while adding the custom field: {ex.Message}");
            }
        }
        public async Task<CustomField> GetOrCreateCustomFieldByNameAsync(string fieldName, CustomField newCustomField)
        {
            // Vérifier si le champ personnalisé existe déjà
            var existingField = await _context.CustomFields
                .FirstOrDefaultAsync(cf => cf.Name == fieldName);

            if (existingField != null)
            {
                // Retourner le champ existant
                return existingField;
            }


            _context.CustomFields.Add(newCustomField);
            await _context.SaveChangesAsync();

            return newCustomField;
        }
        public Task AddRangeAsync(IEnumerable<CustomField> entities, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteAsync(Expression<Func<CustomField, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var CustomfieldToDelete = await _context.CustomFields.Where(predicate).ToListAsync(cancellationToken);

            foreach (var custom in CustomfieldToDelete)
            {
                _context.CustomFields.Remove(custom);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(CustomField entity, CancellationToken cancellationToken = default)
        {
            try
            {
              
                _context.CustomFields.Remove(entity);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);

                throw new CustomFieldException(ex.Message.ToString());
            }
        }

        public async Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var customFields = await _context.CustomFields.FindAsync(id, cancellationToken);

                if (customFields == null)
                {
                    throw new CustomFieldException($"Custom Field with ID {id} not found."); 
                }

                _context.CustomFields.Remove(customFields);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new CustomFieldException(ex.Message.ToString());
            }
        }

        public Task DeleteRangeAsync(IReadOnlyList<CustomField> entities, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _context?.Dispose();
        }

        public async Task<bool> ExistsAsync(Expression<Func<CustomField, bool>> predicate, CancellationToken cancellationToken = default)
        {
            bool customfieldtExists = await _context.CustomFields.AnyAsync(predicate, cancellationToken);

            return customfieldtExists;
        }

        public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            bool customFieldExists = await _context.CustomFields.AnyAsync(cf => cf.Id == id, cancellationToken);

            return customFieldExists;
        }

        public async Task<IReadOnlyList<CustomField>> FindAsync(Expression<Func<CustomField, bool>> predicate, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.CustomFields.Where(predicate).ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new CustomFieldException(ex.Message.ToString());
            }
        }

        public async Task<CustomField?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.CustomFields.FindAsync(id, cancellationToken);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new CustomFieldException(ex.Message.ToString());
            }
        }

        public Task<CustomField?> FindOneAsync(Expression<Func<CustomField, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<IReadOnlyList<CustomField>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var CustomFields = await _context.CustomFields.ToListAsync(cancellationToken).ConfigureAwait(false);

            return CustomFields;
        }
        public async Task<IReadOnlyList<CustomFieldDetails>> GetAllDetailsAsync(CancellationToken cancellationToken = default)
        {
            var customFields = await _context.CustomFields
                                             .Select(cf => new CustomFieldDetails
                                             {
                                                 Id = cf.Id,
                                                 Name = cf.Name,
                                                 Data_type = cf.Data_type
                                             })
                                             .ToListAsync(cancellationToken)
                                             .ConfigureAwait(false);

            return customFields;
        }
        public async Task<PagedList<CustomFieldDetails>> GetPagedCustomFieldAsync<CustomFieldDetails>(CustomFieldParameters customfieldparameters, CancellationToken cancellationToken = default)
        {
            List<Guid> guids = new List<Guid>();
            NewtonSoftService serializer = new NewtonSoftService();
            var query = _context.CustomFields.Select(cf =>new {
                cf.Id,
                cf.Name,
            cf.Data_type}).AsQueryable();
            foreach (var customfield in query)
            {
                guids.Add(customfield.Id);
            }
            int totalItems = await query.CountAsync();
            var items = await query
            .Skip(customfieldparameters.PageSize * (customfieldparameters.Page - 1))
            .Take(customfieldparameters.PageSize)
                .ToListAsync();
            string serializedItems = serializer.Serialize(items);
            List<CustomFieldDetails> customfieldList = serializer.Deserialize<List<CustomFieldDetails>>(serializedItems);
            return new PagedList<CustomFieldDetails>(customfieldList, totalItems, customfieldparameters.Page, customfieldparameters.PageSize, guids);

        }

        public async Task UpdateAsync(CustomField entity, CancellationToken cancellationToken = default)
        {
            try
            {
                var existingEntity = await _context.CustomFields.FindAsync(entity.Id);

                if (existingEntity == null)
                {
                    throw new CustomFieldException($"Custom Field with ID {entity.Id} not found.");
                }

                existingEntity.Name = entity.Name;
                if (existingEntity.Data_type != entity.Data_type)
                {
                    throw new CustomFieldException("Data type cannot be changed after a field is created");
                   
                }

                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new CustomFieldException(ex.Message.ToString());
            }
        }

        public async Task<CustomField> FindByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            CustomField customFieldExists = await _context.CustomFields.FirstOrDefaultAsync(cf => cf.Name == name, cancellationToken);

            return customFieldExists;
        }
      
    }
}
