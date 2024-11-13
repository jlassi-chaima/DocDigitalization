using Application.Dtos.Tag;
using Application.Parameters;
using Application.Respository;

using Core.Events;
using DD.Core.Pagination;
using MediatR;
using System.Security.Claims;
using System.Text.Json;

namespace Application.Features.FeaturesTag
{
    public class ListTag
    {
        public sealed record Query : IRequest<PagedList<TagDtoDetails>>
        {
            public TagParameters tagparameters { get; set; }
            public string? Owner { get; set; }
            public Query(string? owner,TagParameters tagparam)
            {
                Owner  = owner;
                tagparameters = tagparam;
               
            }
        }
        public sealed class Handler : IRequestHandler<Query, PagedList<TagDtoDetails>>
        {
            private readonly ITagRepository _repository;
            private readonly IEventPublisher _eventBus;
            public Handler(ITagRepository repository, IEventPublisher eventBus)
            {
                _repository = repository;
                _eventBus = eventBus;
            }

            public async Task<PagedList<TagDtoDetails>> Handle(Query request, CancellationToken cancellationToken)
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
                
                var tags = await _repository.GetPagedtagAsync<TagDtoDetails>(request.tagparameters,request.Owner, GroupsList, cancellationToken);
               
                return tags;

            }
        }
    }
}
