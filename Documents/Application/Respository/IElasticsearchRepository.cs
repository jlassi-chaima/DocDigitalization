using Domain.Documents;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Respository
{
    public interface IElasticsearchRepository
    {
        Task IndexDocumentAsync(Document document);
        Task DeleteDocumentAsync(Guid id);
        Task<ISearchResponse<Document>> SearchDocumentsAsync(string query);
    }
}
