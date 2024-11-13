

using Application.Parameters;

namespace Application.Dtos.Documents
{
    public class DocumentSearchDto
    {
        public List<Guid>? TagID { get; set; } 
        public List<Guid>? CorrespondentID { get; set; } 
        public List<Guid>? DocumentTypeID { get; set; }
        public List<Guid>? StoragePathID { get; set; } 
        public string? TitleContains { get; set; } 
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public List<string>? Groups { get; set; }

        public string? Owner { get; set; } 
        public string? OwnerId { get; set; } 
        public string? OwnerIdNone { get; set; } 
        public int? OwnerIsNull { get; set; } 
        public required DocumentParameters DocumentParameters { get; set; } =new DocumentParameters();
        public string? Search { get; set; } 
        public string? Ordering { get; set; } 
        public int? ArchiveSerialNumber { get; set; } 
        public int? ArchiveSerialNumberIsNull { get; set; } 
        public int? ArchiveSerialNumberGT { get; set; } 
        public int? ArchiveSerialNumberLT { get; set; } 
        public string? TitleContent { get; set; } 
    }
}
