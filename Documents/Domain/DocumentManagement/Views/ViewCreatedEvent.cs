using Core.Events;

namespace Domain.DocumentManagement.Views
{
    public class ViewCreatedEvent : DomainEvent
    {
      
            public Guid ViewId { get; }
            public string Name { get; }
            public ViewCreatedEvent(Guid viewId, string name)
            {
                ViewId = viewId;
                Name = name;
            }
        
    }
}
