using Domain.Documents;


namespace Domain.DocumentManagement.tags
{
    public class DocumentTags
    { 
        public Guid DocumentId { get; set; }
        public Document Document { get; set; } 
        public Guid TagId { get; set; }
        public Tag Tag { get; set; }
    }
}
