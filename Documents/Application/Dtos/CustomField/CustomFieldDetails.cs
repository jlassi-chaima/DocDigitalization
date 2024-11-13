using Domain.DocumentManagement.CustomFields;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.CustomField
{
    public class CustomFieldDetails
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public TypeField Data_type { get; set; }
    }
}
