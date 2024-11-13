using Core.Events;

namespace Domain.DocumentManagement.Correspondent
{
    public class CorrespondentCreatedEvent : DomainEvent
    {
        public Guid CorrespondentId { get; }
        public string Name { get; }
        public CorrespondentCreatedEvent(Guid correspondentId, string name)
        {
            CorrespondentId = correspondentId;
            Name = name;
        }
    }
}