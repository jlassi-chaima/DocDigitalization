
using Core.Domain;
using Domain.Documents;

namespace Domain.DocumentManagement.StoragePath
{
    public class StoragePath : BaseEntity
    {
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Path { get; set; }
        public List<string>? Match { get; set; }
        public Matching_Algorithms Matching_algorithm { get; set; }
        public bool Is_insensitive { get; set; }
        public int Document_count { get; set; }
        public string? Owner { get; set; }
        public ICollection<Document>? Documents { get; set; } = new List<Document>();

        public List<string>? UsersView { get; set; }
        public List<string>? GroupsView { get; set; }

        public List<string>? UsersChange { get; set; }
        public List<string>? GroupsChange { get; set; }
        //public List<StoragePathDocument> StoragePathDocument { get; set; }

        public static StoragePath Create(
        string name,
        List<string> match,
        string owner,
        bool is_insensitive,
        string path,
        string slug,
        int Document_count,
        Matching_Algorithms algorithm,
        ICollection<Document> document,
List<string>? usersView,
            List<string>? groupsView,
            List<string>? usersChange,
            List<string>? groupsChange
           )
        {
            StoragePath storage = new StoragePath()
            {
                Name = name,
                Match = match,
                Matching_algorithm = algorithm,
                Owner = owner,
                Path = path,
                Slug = slug,
                Is_insensitive = is_insensitive,
                Documents = document ?? new List<Document>(),
                UsersView = usersView,
                GroupsView = groupsView,
                UsersChange = usersChange,
                GroupsChange = groupsChange
            };
            var @event = new StoragePathCreatedEvent(storage.Id, storage.Name);
            storage.AddDomainEvent(@event);
            return storage;
        }
    }
}
