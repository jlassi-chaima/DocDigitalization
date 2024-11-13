

using Application.Parameters;

namespace Application.Dtos.Documents
{
    public class DocumentSearchParameters
    {
        public  string? TagID;
        public  string? CorrespondentID;
        public  string? DocumentTypeID;
        public  string? StoragePathID;
        public  string? TitleIcontains;
        public  string? Created;
        public  string? Owner;
        public  string? OwnerId;
        public  string? OwnerIdNone;
        public  string? Search;
        public  int? OwnerIsNull;
        public  string? Ordering;
        public  int? ArchiveSerialNumber;
        public  int? ArchiveSerialNumberIsNull;
        public  int? ArchiveSerialNumberGT;
        public  int? ArchiveSerialNumberLT;
        public  string? TitleContent;

        public DocumentParameters DocumentParameters { get; set; }
    }
}
