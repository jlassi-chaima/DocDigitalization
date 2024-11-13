using Application.Dtos.StoragePath;
using Application.Parameters;
using Application.Respository;
using DD.Core.Pagination;
using MediatR;
using System.Text.Json;


namespace Application.Features.FeaturesStoragePath
{
    public class ListStoragePath
    {
        public sealed record Query : IRequest<PagedList<StoragePathDto>>
        {
            public StoragePathParameters storagepathparameters { get; set; }
            public string? Owner { get; set; }
            public Query(string? owner, StoragePathParameters storagepathparam)
            {
                storagepathparameters = storagepathparam;
                Owner = owner;
            }
        }
        public sealed class Handler : IRequestHandler<Query, PagedList<StoragePathDto>>
        {
            private readonly IStoragePathRepository _repository;

            public Handler(IStoragePathRepository repository)
            {
                _repository = repository;
            }

            public async Task<PagedList<StoragePathDto>> Handle(Query request, CancellationToken cancellationToken)
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
                return await _repository.GetPagedStoragePathAsync<StoragePathDto>(request.storagepathparameters, request.Owner, GroupsList, cancellationToken);
            }
        }
    }
}