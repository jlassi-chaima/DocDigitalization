using Core.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DocumentManagement.DocumentTypes
{
    public class DocumentTypeCreatedEvent : DomainEvent
    {
        public Guid DocumentTypeId { get; }
        public string Name { get; }
        public DocumentTypeCreatedEvent(Guid documentTypeId, string name)
        {
            DocumentTypeId = documentTypeId;
            Name = name;
        }

    }
}
