using Application.Dtos.Documents;
using Application.Parameters;
using Core.Database;
using DD.Core.Pagination;
using Domain.Documents;


namespace Application.Respository
{
    public interface IDocumentRepository : IRepository<Document, Guid>
    {
        Task<PagedList<DocumentDetailsDTO>> GetPagedDocumentAsync<DocumentDetailsDTO>(DocumentParameters documentparameters , CancellationToken cancellationToken = default);
        Task<Document> FindByIdDetailsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<DocumentSuggestionsDto> FindByIdDetailsSuggestionsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<Document>> GetAllDocumentByGroup(Guid groupId);
        Task<List<Document>> GetAllDocuments();        
        Task<List<Document>> GetAllDocumentByOwner(string owner);
        Task<PagedList<DocumentDetailsDTO>> GetDocumentsByTagID<DocumentDetailsDTO>(DocumentParameters documentparameters, List<Guid>? Id, CancellationToken cancellationToken = default);
        Task<PagedList<DocumentDetailsDTO>> GetDocumentsByCorrespondentID<DocumentDetailsDTO>(DocumentParameters documentparameters, List<Guid>? Id, CancellationToken cancellationToken = default);
        Task<PagedList<DocumentDetailsDTO>> GetDocumentsByDocumentTypeID<DocumentDetailsDTO>(DocumentParameters documentparameters, List<Guid>? Id, CancellationToken cancellationToken = default);
        Task<PagedList<DocumentDetailsDTO>> GetDocumentByTagCorrespondentDocumentType<DocumentDetailsDTO>(DocumentSearchDto documentSearch, Guid groupId,CancellationToken cancellationToken = default);
        Task IndexDocumentToElasticsearchAsync(Document document);
        Task<Document> GetDocumentByGroupIdAndASN(Guid groupId, string baseWordASN);
    }
    
}
