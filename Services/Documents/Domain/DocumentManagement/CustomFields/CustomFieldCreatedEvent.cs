using Core.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DocumentManagement.CustomFields
{
    public class CustomFieldCreatedEvent : DomainEvent
    {
        public Guid CustomFieldId { get; }
        public string Name { get; }
        public CustomFieldCreatedEvent(Guid customFieldId, string name)
        {
            CustomFieldId = customFieldId;
            Name = name;
        }
    }
}
