using Application.Dtos.DocumentType;
using Application.Parameters;
using Application.Respository;
using DD.Core.Pagination;
using MediatR;
using System.Text.Json;

namespace Application.Features.FeaturesDocumentType
{
    public class ListDocumentType
    {
        public sealed record Query : IRequest<PagedList<DocumentTypeDetailsDTO>>
        {
            public DocumentTypeParameters documenttypeparameters { get; set; }
            public string? Owner { get; set; }
            public Query(string? owner,DocumentTypeParameters doctypeparam)
            {
                documenttypeparameters = doctypeparam;
                Owner = owner;

            }
        }
        public sealed class Handler : IRequestHandler<Query, PagedList<DocumentTypeDetailsDTO>>
        {
            private readonly IDocumentTypeRepository _repository;

            public Handler(IDocumentTypeRepository repository)
            {
                _repository = repository;
            }

            public async Task<PagedList<DocumentTypeDetailsDTO>> Handle(Query request, CancellationToken cancellationToken)
            {
                // get list groupe by id user 
                var apiUrl = "http://localhost:5183/user/getlistGroups";
                // Append the result as a query parameter to the URL
                apiUrl += "?id=" + Uri.EscapeDataString(request.Owner);
                List<string> GroupsList;
                using (var httpClient = new HttpClient())
                {

                    var response = await httpClient.GetAsync(apiUrl);

                    // Ensure the request completed successfully
                    response.EnsureSuccessStatusCode();

                    // Read the response content as a string
                    var responseBody = await response.Content.ReadAsStringAsync();
                    GroupsList = JsonSerializer.Deserialize<List<string>>(responseBody, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                }
                return await _repository.GetPagedDocumentTypeAsync<DocumentTypeDetailsDTO>(request.documenttypeparameters, request.Owner, GroupsList, cancellationToken);
            }
        }
    }
}
