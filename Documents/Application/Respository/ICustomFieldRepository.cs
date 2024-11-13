using Application.Dtos.CustomField;
using Application.Parameters;
using Core.Database;
using DD.Core.Pagination;
using Domain.DocumentManagement.CustomFields;


namespace Application.Respository
{
    public interface ICustomFieldRepository : IRepository<CustomField,Guid>
    {
   
        Task<IReadOnlyList<CustomFieldDetails>> GetAllDetailsAsync(CancellationToken cancellationToken = default);
        Task<PagedList<CustomFieldDetails>> GetPagedCustomFieldAsync<CustomFieldDetails>(CustomFieldParameters customfieldparameters, CancellationToken cancellationToken = default);
        Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<CustomField> FindByNameAsync(string name, CancellationToken cancellationToken = default);
    }
}
