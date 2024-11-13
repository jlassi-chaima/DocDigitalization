using Application.Respository;
using Domain.DocumentManagement.Correspondent;
using Domain.DocumentManagement.DocumentTypes;
using Domain.DocumentManagement.StoragePath;
using Domain.DocumentManagement.tags;
using Domain.Documents;
using MediatR;
using System.Net.Http;
using System.Text.Json;

namespace Application.Statistics.FeaturesStatistics
{
    public class StatisticsDetails
    {
        public sealed record Query : IRequest<Model.Statistics>
        {

        }
        public sealed class Handler : IRequestHandler<Query, Model.Statistics>
        {
            private readonly IDocumentRepository _repository;
            private readonly ITagRepository _tagRepository;
            private readonly ICorrespondentRepository _correspondentRepository;
            private readonly IDocumentTypeRepository _documentTypeRepository;
            private readonly IStoragePathRepository _storagePathRepository;
            private readonly HttpClient _httpClient;

            public Handler(IDocumentRepository repository,
                ITagRepository tagRepository,
                ICorrespondentRepository correspondentRepository,
                IDocumentTypeRepository documentTypeRepository,
                IStoragePathRepository storagePathRepository,
                HttpClient httpClient)
            {
                _repository = repository;
                _tagRepository = tagRepository;
                _correspondentRepository = correspondentRepository;
                _documentTypeRepository = documentTypeRepository;
                _storagePathRepository = storagePathRepository;
                _httpClient = httpClient;
            }

            public async Task<Model.Statistics> Handle(Query request, CancellationToken cancellationToken)
            {
                int document_total = 0;
                int character_count = 0;
                int tag_count = 0;
                int correspondent_count = 0;
                int document_type_count = 0;
                int storage_path_count = 0;
                int users_count = 0;
                int groups_count = 0;

                List<Document> documents = (List<Document>)await _repository.GetAllAsync();
                document_total += documents.Count;
                foreach (Document document in documents)
                {
                    character_count += document.Content.Length;
                }

                List<Tag> tags = (List<Tag>)await _tagRepository.GetAllAsync();
                tag_count += tags.Count;

                List<Correspondent> correspondents = (List<Correspondent>)await _correspondentRepository.GetAllAsync();
                correspondent_count += correspondents.Count;

                List<DocumentType> documentTypes = (List<DocumentType>)await _documentTypeRepository.GetAllAsync();
                document_type_count += documentTypes.Count;

                List<StoragePath> storagePaths = (List<StoragePath>)await _storagePathRepository.GetAllAsync();
                storage_path_count += storagePaths.Count;

                // Fetch users by email from the external service
                var response_user = await _httpClient.GetAsync("http://localhost:5183/user/list_user");
                response_user.EnsureSuccessStatusCode();
                var usersJson = await response_user.Content.ReadAsStringAsync();
                var users = JsonSerializer.Deserialize<List<User>>(usersJson);
                users_count += users.Count;



                // Fetch users by email from the external service
                var response_group = await _httpClient.GetAsync("http://localhost:5183/group/list_groups");
                response_group.EnsureSuccessStatusCode();
                var groupsJson = await response_group.Content.ReadAsStringAsync();
                var groups = JsonSerializer.Deserialize<List<Group>>(groupsJson);
                groups_count += groups.Count;




                return Model.Statistics.Create(
                    document_total,
                    character_count,
                    tag_count,
                    correspondent_count,
                    document_type_count,
                    storage_path_count,
                    users_count,
                    groups_count
                );
            }
        }
        // User class in the other microservice
        public class User
        {

            public string Username { get; set; }
            // other properties
        }

        // Group class in the other microservice
        public class Group
        {

            public string GroupName { get; set; }
            // other properties
        }

    }
}
