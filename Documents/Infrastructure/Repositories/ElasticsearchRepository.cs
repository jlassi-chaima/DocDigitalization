using Application.Respository;
using Domain.Documents;
using Microsoft.Extensions.Configuration;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class ElasticsearchRepository : IElasticsearchRepository
    {
        private readonly IElasticClient _elasticClient;
        private readonly string _index;

        public ElasticsearchRepository(IElasticClient elasticClient, IConfiguration configuration)
        {
            _elasticClient = elasticClient;
            _index = configuration["ElasticsearchSettings:Index"];
        }

        public async Task IndexDocumentAsync(Document document)
        {
            var response = await _elasticClient.IndexDocumentAsync(document);
            if (!response.IsValid)
            {
                throw new Exception($"Failed to index document: {response.OriginalException.Message}");
            }
        }

        public async Task DeleteDocumentAsync(Guid id)
        {
            var response = await _elasticClient.DeleteAsync<Document>(id);
            if (!response.IsValid)
            {
                throw new Exception($"Failed to delete document: {response.OriginalException.Message}");
            }
        }

        public async Task<ISearchResponse<Document>> SearchDocumentsAsync(string query)
        {
            var response = await _elasticClient.SearchAsync<Document>(s => s
                .Query(q => q
                    .QueryString(qs => qs
                        .Query(query)
                    )
                )
            );

            if (!response.IsValid)
            {
                throw new Exception($"Search query failed: {response.OriginalException.Message}");
            }

            return response;
        }
    }

}
