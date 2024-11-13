using Application.Parameters;
using Core.Database;
using DD.Core.Pagination;
using Domain.DocumentManagement.Views;

namespace Application.Respository
{
    public interface IViewRepository : IRepository<View, Guid>
    {

        Task<View?> FindByNameAsync(string name, string owner, CancellationToken cancellationToken = default);
        Task<PagedList<View>> GetViewsAsync<ViewDto>(ViewParameters viewParameters, string owner, string? nameIcontains);
    }
}
