using Core.Events;


namespace Domain.DocumentManagement.tags
{
    public class TagCreatedEvent : DomainEvent
    {
        public Guid TagId { get; }
        public string Name { get; }
        public TagCreatedEvent(Guid tagId, string name)
        {
            TagId = tagId;
            Name = name;
        }
    }
}
