using Application.Dtos.Correspondent;
using Application.Dtos.SelectionData;
using Application.Parameters;
using Core.Database;
using DD.Core.Pagination;
using Domain.DocumentManagement.Correspondent;


namespace Application.Respository
{
    public interface ICorrespondentRepository : IRepository<Correspondent, Guid>
    {
        Task<PagedList<Correspondent>> GetPagedCorrespondentAsync<Correspondent>(CorrespondentParameters correspondentparameters, string owner, List<string>? GroupsList, CancellationToken cancellationToken = default);
        Task<PagedList<CorrespondentListDTO>> GetCorrespondentsByNameAsync(CorrespondentParameters correspondentparameters, string name, string owner, CancellationToken cancellationToken = default);
        Task<List<CorrespondentListDTO>> GetCorrespondentDetailsAsync<CorrespondentListDTO>( CancellationToken cancellationToken = default);
        Task<List<DetailsDTO>> SelectionDataCorrespondent(SelectionDataDocuments Ids,string owner);
        Task<List<CorrespondentListDTO>> GetListStoragePathByOwner(string owner);
        Task<Correspondent> FindByUsernameAsync(string username, CancellationToken cancellationToken = default);
        Task<bool> ExistsByIdAsync(Guid correspondentId, CancellationToken cancellationToken = default);
    }
}
