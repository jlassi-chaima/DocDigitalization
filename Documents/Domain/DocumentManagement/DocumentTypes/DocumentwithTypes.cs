using Domain.Documents;

namespace Domain.DocumentManagement.DocumentTypes
{
    public class DocumentwithTypes
    {
        public Guid DocumentId { get; set; }
        public Document Document { get; set; }
        public Guid DocumentTypeId { get; set; }
        public DocumentType DocumentType { get; set; }
    }
}