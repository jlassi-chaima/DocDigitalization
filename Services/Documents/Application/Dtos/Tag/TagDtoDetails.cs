using Application.Dtos.Permission;
using Domain.DocumentManagement;


namespace Application.Dtos.Tag
{
    public class TagDtoDetails
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public List<string> Match { get; set; }
        public Matching_Algorithms matching_algorithm { get; set; }

        public bool Is_insensitive { get; set; }

        public bool Is_inbox { get; set; }

        public string Owner { get; set; }

        public int Document_count { get; set; }

        public PermissionDto? permissions { get; set; }
    }
}
