using Application.Dtos.Permission;
using Domain.DocumentManagement;
using Domain.Documents;


namespace Application.Dtos.StoragePath
{
    public class StoragePathDto
    {

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Path { get; set; }
        public List<string>? Match { get; set; }
        public Matching_Algorithms Matching_algorithm { get; set; }
        public bool Is_insensitive { get; set; }
        public int Document_count { get; set; } = 0;
        public string? Owner { get; set; }
        public ICollection<Document>? Documents { get; set; } = new List<Document>();
        public PermissionDto? permissions { get; set; }

    }
}
