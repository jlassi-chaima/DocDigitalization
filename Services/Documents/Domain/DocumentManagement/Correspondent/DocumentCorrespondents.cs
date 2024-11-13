using Domain.Documents;

namespace Domain.DocumentManagement.Correspondent
{
    public class DocumentCorrespondents
    {
        public Guid DocumentId { get; set; }
        public Document Document { get; set; }
        public Guid CorrespondentId { get; set; }
        public Correspondent Correspondent { get; set; }
    }
}
