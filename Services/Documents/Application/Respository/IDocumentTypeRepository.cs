using Application.Dtos.DocumentType;
using Application.Dtos.SelectionData;
using Application.Parameters;
using Core.Database;
using DD.Core.Pagination;
using Domain.DocumentManagement.DocumentTypes;



namespace Application.Respository
{
    public interface IDocumentTypeRepository : IRepository<DocumentType, Guid>
    {
        Task<PagedList<DocumentTypeDetailsDTO>> GetPagedDocumentTypeAsync<DocumentTypeDetailsDTO>(DocumentTypeParameters documenttypeparameters, string owner, List<string>? GroupsList, CancellationToken cancellationToken = default);
        Task<List<DocumentTypeDetailsDTO>> GetListDocumentTypeByOwner(string owner);
        Task<PagedList<DocumentTypeDetailsDTO>> ListDocumentTypeDropDown<DocumentTypeDetailsDTO>(DocumentTypeParameters documenttypeparameters, string owner,  CancellationToken cancellationToken = default);
        
         Task<PagedList<DocumentTypeDetailsDTO>> GetDocumentTypesByNameAsync(string name, DocumentTypeParameters documenttypeparameters, string owner, CancellationToken cancellationToken = default);
        Task<List<DocumentTypeDetailsDTO>> GetDocumentTypesDetailsAsync(CancellationToken cancellationToken = default);
        Task<List<DocumentTypeDetailsDTO>> GetDocumentTypesDetailsWithMatchingAlgoAutoAsync(CancellationToken cancellationToken = default);

        Task<List<DetailsDTO>> SelectionDataDocumentTypes(SelectionDataDocuments Ids);
        Task<DocumentType?> FindByName(string name ,CancellationToken cancellationToken = default);
    }
}
