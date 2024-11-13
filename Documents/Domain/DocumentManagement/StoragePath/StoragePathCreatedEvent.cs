using Core.Events;

namespace Domain.DocumentManagement.StoragePath
{
    public class StoragePathCreatedEvent : DomainEvent
    {
        public Guid StoragePathId { get; }
        public string Name { get; }
        public StoragePathCreatedEvent(Guid storagePathId, string name)
        {
            StoragePathId = storagePathId;
            Name = name;
        }
    }
}