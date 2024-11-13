namespace Application.Dtos.DocumentCustomField
{
    public class AssignCustomFieldToDocumentDto
    {
        public Guid DocumentId { get; set; }
        public Guid CustomFieldId { get; set; }
    }
}
