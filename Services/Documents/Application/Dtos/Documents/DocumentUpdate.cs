using Application.Dtos.DocumentNote;
using Application.Dtos.Permission;


namespace Application.Dtos.Documents
{
    public  class DocumentUpdate
    {
        public string Title { get; set; }
        public string? Archive_Serial_Number { get; set; }
        public List<DocumentNoteDto>? Notes { get; set; }
        public List<CustomFieldValue>? Custom_Fields { get; set; }
        public List<Guid>? Tags { get; set; }
        public Guid? DocumentTypeId { get; set; }
        public Guid? CorrespondentId { get; set; }
        public Guid? StoragePathId { get; set; }
        public PermissionDto? set_permissions { get; set; }//SetPermission 
    }
}
