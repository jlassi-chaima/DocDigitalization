using Application.Dtos.SelectionData;
using Application.Dtos.Tag;
using Application.Parameters;
using Core.Database;
using DD.Core.Pagination;
using Domain.DocumentManagement.tags;



namespace Application.Respository
{
    public interface ITagRepository : IRepository<Tag, Guid>
    {
        Task<List<Tag>> FindByListIdsAsync(IEnumerable<Guid> tagIds, CancellationToken cancellationToken = default);

        Task<PagedList<TagDtoDetails>> GetPagedtagAsync<TagDtoDetails>(TagParameters tagparameters,string owner,List<string>?  GroupsList,  CancellationToken cancellationToken = default);
        Task<List<Tag>> GetListTagsByOwner(string owner);  
        Task<PagedList<TagDtoDetails>> GetTagsByNameAsync(TagParameters tagparameters,string name, string owner, CancellationToken cancellationToken = default);
        Task<List<TagDtoDetails>> GetTagDetailsAsync(CancellationToken cancellationToken = default);
        Task<List<DetailsDTO>> SelectionDataTag(SelectionDataDocuments Ids, string owner);
    }
}
