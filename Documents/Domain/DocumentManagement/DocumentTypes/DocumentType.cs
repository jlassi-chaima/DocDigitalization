using Core.Domain;
using Domain.DocumentManagement;
using Domain.Documents;

namespace Domain.DocumentManagement.DocumentTypes
{
    public class DocumentType : BaseEntity
    {
        public string Name { get; set; }
        public string Slug { get; set; }
        public List<string>? Match { get; set; } =new List<string>();
        public Matching_Algorithms Matching_algorithm { get; set; } = Matching_Algorithms.MATCH_NONE;
        public bool Is_insensitive { get; set; } = true;
        public string Owner { get; set; }
        public int Document_count { get; set; } = 0;
        public List<Document> Documents { get; set; }
        //Custom Fields
        public List<Guid>? ExtractedData { get; set; }

        public List<string>? UsersView { get; set; }
        public List<string>? GroupsView { get; set; }

        public List<string>? UsersChange { get; set; }
        public List<string>? GroupsChange { get; set; }

        public static DocumentType Create(
        string name,
        string slug,
        string owner,
        bool is_sensitive,
        Matching_Algorithms algorithm,
        List<string>? usersView,
        List<string>? groupsView,
        List<string>? usersChange,
        List<string>? groupsChange



           )
        {
            DocumentType type = new DocumentType()
            {
                Name = name,
                Slug = slug,
                Matching_algorithm = algorithm,
                Owner = owner,
                Is_insensitive = is_sensitive,
                UsersView = usersView,
                GroupsView = groupsView,
                UsersChange = usersChange,
                GroupsChange = groupsChange

            };
            var @event = new DocumentTypeCreatedEvent(type.Id, type.Name);
            type.AddDomainEvent(@event);
            return type;
        }
    }
}
