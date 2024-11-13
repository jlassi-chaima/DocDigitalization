using Core.Domain;
using Domain.DocumentManagement;
using Domain.Documents;

namespace Domain.DocumentManagement.Correspondent
{
    public class Correspondent: BaseEntity
    {
        public string Name { get; set; }
        public string Slug { get; set; }
        public List<string>? Match { get; set; }
        public Matching_Algorithms Matching_algorithm { get; set; } = Matching_Algorithms.MATCH_NONE;
        public bool Is_insensitive { get; set; }
        public string Owner { get; set; }
        public int Document_count { get; set; } = 0;
        public DateTime? Last_correspondence { get; set; }
        public List<Document>? Documents { get; set; }

        public List<string>? UsersView { get; set; }
        public List<string>? GroupsView { get; set; }

        public List<string>? UsersChange { get; set; }
        public List<string>? GroupsChange { get; set; }
        public static Correspondent Create(
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
            Correspondent type = new Correspondent()
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
            var @event = new CorrespondentCreatedEvent(type.Id, type.Name);
            type.AddDomainEvent(@event);
            return type;
        }
    }
}
