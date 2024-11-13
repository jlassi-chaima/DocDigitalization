using Application.Dtos.Permission;
using Domain.DocumentManagement;


namespace Application.Dtos.Tag
{
        public class TagDto
        {
            public string Name { get; set; }
            public string Color { get; set; }

            public List<string> Match { get; set; }
            public Matching_Algorithms Matching_algorithm { get; set; }

            public bool Is_insensitive { get; set; }

            public bool Is_inbox { get; set; }

            public string Owner { get; set; }

            public PermissionDto Set_permissions { get; set; }


    }
}
