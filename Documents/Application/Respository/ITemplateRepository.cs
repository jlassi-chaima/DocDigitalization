using Application.Dtos.Templates;
using Application.Parameters;
using Core.Database;
using DD.Core.Pagination;
using Domain.Templates;


namespace Application.Respository
{
    public interface ITemplateRepository: IRepository<Template, Guid>
    {
        Task<List<Template>> GetAllByOrderAsync(CancellationToken cancellationToken = default);
        Task<PagedList<PagedTemplate>> GetPagedTemplatesAsync(TemplateParametres templateparameters, string owner, CancellationToken cancellationToken = default);
        Task<List<Template>> GetAllByOwner(string id);
    }
}
