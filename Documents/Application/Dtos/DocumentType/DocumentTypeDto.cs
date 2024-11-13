using Application.Dtos.Permission;
using Domain.DocumentManagement;
using System.Runtime.InteropServices;

namespace Application.Dtos.DocumentType
{
    public class DocumentTypeDto
    {
        public string Name { get; set; }
        public List<string>? Match { get; set; }
        public Matching_Algorithms Matching_algorithm { get; set; }
        public bool Is_insensitive { get; set; }
        public string Owner { get; set; }
        public List<Guid>? ExtractedData { get; set; }

        public PermissionDto? Set_permissions { get; set; }

    }
}
