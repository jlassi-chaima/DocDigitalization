using Application.Dtos.ArchivenSerialNumber;
using Application.Dtos.Correspondent;
using Application.Dtos.View;
using Application.Exceptions;
using Application.Parameters;
using Application.Respository;
using DD.Core.Pagination;
using Domain.DocumentManagement.Views;
using Infrastructure.Persistence;
using Infrastructure.Serializers;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Linq.Expressions;

namespace Infrastructure.Repositories
{
    public class ViewRepository : IViewRepository
    {
        private readonly DBContext _context;

        public ViewRepository(DBContext context)
        {
            _context = context;
        }
        public async Task AddAsync(View entity, CancellationToken cancellationToken = default)
        {
            try
            {
                await _context.AddAsync(entity, cancellationToken);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error($"Error message: {ex.Message}");
                throw new ViewException(ex.Message.ToString());
            }

        }

        public Task AddRangeAsync(IEnumerable<View> entities, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(Expression<Func<View, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteAsync(View entity, CancellationToken cancellationToken = default)
        {
            try
            {
                _context.Views.Remove(entity);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                Log.Error($"Error message: {ex.Message}");
                throw new ViewException(ex.ToString());
            }
        }

        public Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteRangeAsync(IReadOnlyList<View> entities, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _context?.Dispose();
        }

        public Task<bool> ExistsAsync(Expression<Func<View, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<View>> FindAsync(Expression<Func<View, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<View?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Views.FindAsync(id, cancellationToken);
            }
            catch (Exception ex)
            {
                Log.Error($"Error message: {ex.Message}");
                throw new ViewException(ex.Message.ToString());
            }
        }
        public async Task<View?> FindByNameAsync(string name, string owner, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Views.FirstOrDefaultAsync(v=>v.Name==name && v.Owner==owner, cancellationToken);
            }
            catch (Exception ex)
            {
                Log.Error($"Error message: {ex.Message}");
                throw new ViewException(ex.Message.ToString());
            }
        }
        public Task<View?> FindOneAsync(Expression<Func<View, bool>> predicate, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<PagedList<View>> GetViewsAsync<ViewDto>(ViewParameters viewParameters, string owner, string? nameIcontains)
        {
           
                List<Guid> guids = new List<Guid>();
                NewtonSoftService serializer = new NewtonSoftService();
                try
                {
                var views = _context.Views.Include(view => view.Tag).Select(t => new {
                    t.Id,
                    t.Name,
                    t.StartDate,
                    t.EndDate,
                    t.Owner,
                    t.Tag
                }).Where(view => (string.IsNullOrEmpty(nameIcontains) || view.Name.Contains(nameIcontains)) && view.Owner == owner)
                                             .AsQueryable();
                    foreach (var view in views)
                    {
                        guids.Add(view.Id);
                    }
                    int count = await views.CountAsync();

                    var items = await views
                        .Skip(viewParameters.PageSize * (viewParameters.Page - 1))
                        .Take(viewParameters.PageSize)
                        .ToListAsync();

                    string serializedCorrespondent = serializer.Serialize(items);
                    List<View> deserializedCorrespondent = serializer.Deserialize<List<View>>(serializedCorrespondent);
                    return new PagedList<View>(deserializedCorrespondent, count, viewParameters.Page, viewParameters.PageSize, guids);
                }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                throw new ViewException(ex.Message.ToString());

            }
           
            
        }

        public Task<IReadOnlyList<View>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateAsync(View entity, CancellationToken cancellationToken = default)
        {
            try
            {

                var existingEntity = await _context.Views.FindAsync(entity.Id);

                if (existingEntity == null)
                {
                    Log.Error($"View with ID {entity.Id} not found.");
                    throw new ViewException($"View with ID {entity.Id} not found.");
                }

                existingEntity.Name = entity.Name;
                existingEntity.TagId = entity.TagId;
                existingEntity.StartDate = entity.StartDate;
                existingEntity.EndDate = entity.EndDate;

                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                Log.Error($"Error message: {ex.Message}");
                throw new ViewException(ex.Message.ToString());
            }
        }
    }
}
