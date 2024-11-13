
using Core.Domain;
using Domain.DocumentManagement;


namespace Domain.DocumentManagement.tags
{
    public class Tag : BaseEntity
    {
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Color { get; set; }
        public List<string>? Match { get; set; }
        public Matching_Algorithms Matching_algorithm { get; set; } = Matching_Algorithms.MATCH_NONE;
        public bool Is_insensitive { get; set; }
        public bool Is_inbox { get; set; } = false;
        public string Owner { get; set; }
        public List<DocumentTags>? Documents { get; set; }
        public int Document_count { get; set; } = 0;

        public List<string>? UsersView { get; set; }
        public List<string>? GroupsView { get; set; }

        public List<string>? UsersChange { get; set; }
        public List<string>? GroupsChange { get; set; }


        public static Tag Create(
             string name,
             string slug,
             string color,
             string owner,
             bool is_insensitive,
             Matching_Algorithms algorithm,
            List<string>? usersView,
            List<string>? groupsView,
            List<string>? usersChange,
            List<string>? groupsChange

            )
            {
                Tag tag = new()
                {
                    Name = name,
                    Color = color,
                    Slug = slug,
                    Matching_algorithm = algorithm,
                    Owner = owner,
                    Is_insensitive = is_insensitive,
                    UsersView = usersView,
                    GroupsView = groupsView,
                    UsersChange = usersChange,
                    GroupsChange = groupsChange
                };
                var @event = new TagCreatedEvent(tag.Id, tag.Name);
                tag.AddDomainEvent(@event);
                return tag;
        }
        public Tag()
        {
        }
    }
}
