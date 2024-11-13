using Core.Domain;
using Domain.DocumentManagement.DocumentTypes;
using Domain.DocumentManagement.tags;

namespace Domain.DocumentManagement.CustomFields
{
    public class CustomField : BaseEntity
    {
        public  string Name { get; set; }

        public TypeField Data_type { get; set; }

        public ICollection<DocumentCustomField>? DocumentsCustomFields { get; set; } = new List<DocumentCustomField>();


        public static CustomField Create(
        string name,
        TypeField dataType        

           )
        {
            CustomField type = new CustomField()
            {
                Name = name,
                Data_type = dataType,
            };
            var @event = new DocumentTypeCreatedEvent(type.Id, type.Name);
            type.AddDomainEvent(@event);
            return type;
        }
    }
}
