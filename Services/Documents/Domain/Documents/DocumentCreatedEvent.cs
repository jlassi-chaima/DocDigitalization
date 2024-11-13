using Core.Events;


namespace Domain.Documents
{
    public class DocumentCreatedEvent : DomainEvent
    {
        public Guid DocumentId { get; }
        public string Title { get; }
        public DocumentCreatedEvent(Guid documentId, string title)
        {
            DocumentId = documentId;
            Title = title;
        }


    }
}
