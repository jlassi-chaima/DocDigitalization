using Domain.DocumentManagement.CustomFields;

namespace Application.Dtos.CustomField
{
    public class CustomFieldDto
    {
        public string Name { get; set; }

        public TypeField Data_type { get; set; }

    }
}
