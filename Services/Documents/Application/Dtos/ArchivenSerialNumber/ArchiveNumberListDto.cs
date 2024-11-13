

using Application.Dtos.Permission;

namespace Application.Dtos.ArchivenSerialNumber
{
    public class ArchiveNumberListDto
    {
        public Guid Guid { get; set; }
        public required string Prefix { get; set; }
        public required string GroupName { get; set; }
        public required Guid GroupId { get; set; }
        public required DateOnly Year { get; set; }
        public string? Owner { get; set; }
        public int DocumentCount { get; set; }
       // public PermissionDto? permissions { get; set; }
    }
}
