using Domain.Documents;

namespace Domain.DocumentManagement.CustomFields
{
    public class DocumentCustomField
    {
        public string? Value { get; set; }
        public Guid DocumentId { get; set; }
        public virtual Document Document { get; set; }
        public Guid CustomFieldId { get; set; }
        public virtual CustomField CustomField { get; set; }
            
    }
}