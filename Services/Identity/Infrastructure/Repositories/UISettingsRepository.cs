using Application.Repository;
using System.Linq.Expressions;
using Infrastructure.Persistence;
using Domain.Settings;
using Application.Exceptions;
using Microsoft.EntityFrameworkCore;


namespace Infrastructure.Repositories
{
    public class UISettingsRepository : IUISettingsRepository
    {
        private readonly DBContext _context;

        public UISettingsRepository(DBContext context)
        {
            _context = context;
        }
        public async Task AddAsync(UISettings entity, CancellationToken cancellationToken = default)
        {
            try
            {
                 _context.Add(entity);
                 await _context.SaveChangesAsync();

            }
            catch (Exception ex)
            {
               throw new Exception(ex.Message.ToString());
            }
        }

        public Task AddRangeAsync(IEnumerable<UISettings> entities, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(Expression<Func<UISettings, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(UISettings entity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteRangeAsync(IReadOnlyList<UISettings> entities, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _context?.Dispose();
        }

        public Task<bool> ExistsAsync(Expression<Func<UISettings, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<UISettings>> FindAsync(Expression<Func<UISettings, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<UISettings?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                UISettings x = await _context.UISettings
                .Include(u => u.User)
                                      .Where(s => s.User.Id == id.ToString())
                                      .FirstOrDefaultAsync(cancellationToken);
                return x;
            }
            catch (Exception ex)
            {
                throw new UISettingsException(ex.Message.ToString());
            }
        }

        public Task<UISettings?> FindOneAsync(Expression<Func<UISettings, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<UISettings>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateAsync(UISettings entity, CancellationToken cancellationToken = default)
        {
            try
            {
                // Retrieve the existing UISettings entity from the database
                var existingEntity = await _context.UISettings.FindAsync(entity.Id);

                if (existingEntity == null)
                {
                    // Handle case where entity with the given Id does not exist
                    throw new Exception("UISettings entity not found.");
                }

                // Update properties of the existing entity
                existingEntity.Tour_complete = entity.Tour_complete;
                existingEntity.DocumentListSize = entity.DocumentListSize;
                existingEntity.DarkMode_use_system = entity.DarkMode_use_system;
                existingEntity.DarkMode_enabled = entity.DarkMode_enabled;
                existingEntity.DarkMode_thumb_inverted = entity.DarkMode_thumb_inverted;
                existingEntity.Notes_enabled = entity.Notes_enabled;
                existingEntity.Language = entity.Language;
                existingEntity.Default_view_users = entity.Default_view_users;
                existingEntity.Default_view_groups = entity.Default_view_groups;
                existingEntity.Default_edit_users = entity.Default_edit_users;
                existingEntity.Default_edit_groups = entity.Default_edit_groups;
                existingEntity.User = entity.User;
                existingEntity.Settings = entity.Settings; // Update the Settings property

                // Save the changes to the database
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new UISettingsException($"Error updating UISettings entity: {ex.Message}");
            }

        }
    }
}
