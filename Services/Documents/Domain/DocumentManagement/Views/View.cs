
using Core.Domain;
using Domain.DocumentManagement.tags;

namespace Domain.DocumentManagement.Views
{
    public class View : BaseEntity
    {
        public required  string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Guid TagId { get; set; }
        public Tag? Tag { get; set; }
        public required string Owner { get; set; }

        public static View Create(
             string name,
             Guid tagId,
             string owner
            
            )
        {
            View view = new()
            {
                Name = name,
                TagId = tagId,
                Owner= owner


            };
            var @event = new ViewCreatedEvent(view.Id,view.Name);
            view.AddDomainEvent(@event);
            return view;
        }
        public View()
        {
        }
    }
}
