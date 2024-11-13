
using Domain.DocumentManagement;

namespace Application.Dtos.StoragePath
{
    public class ListStoragePathDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Path { get; set; }
        public List<string>? Match { get; set; }
        public Matching_Algorithms Matching_algorithm { get; set; }
        public bool Is_insensitive { get; set; }
        public int Document_count { get; set; }
        public string? Owner { get; set; }
    }
}
