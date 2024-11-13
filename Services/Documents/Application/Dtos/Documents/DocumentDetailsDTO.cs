using Application.Dtos.CustomField;
using Application.Dtos.DocumentNote;
using Application.Dtos.Permission;
using Application.Dtos.Tag;
using System.Text.Json.Serialization;

namespace Application.Dtos.Documents
{
    public  class DocumentDetailsDTO
    {

        public Guid Id { get; set; } 
        public string Title { get; set; }
        public string Content { get; set; }
        public string Base64Data { get; set; }
        public string MimeType { get; set; }

        //public string FileData { get; set; }
        public string? Owner { get; set; }
        public string? FileData { get; set; }
        public string Archive_serial_number { get; set; }
        public DateTime? CreatedOn { get; set; }
        public Guid? DocumentTypeId { get; set; }
        public Guid? StoragePathId { get; set; } 
        public Guid? GroupId { get; set; }


        public PermissionDto? Permissions { get; set; }=new PermissionDto();

        public Guid? CorrespondentId { get; set; }
        public List<Guid>? Tags { get; set; }=new List<Guid>();
        public List<DocumentCustomFieldDTO>? Custom_fields { get; set; }

        public List<DocumentNoteDto>? Notes { get; set; }
        

    }
}
