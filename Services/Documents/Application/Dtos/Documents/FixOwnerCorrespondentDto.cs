using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Documents
{
    public class FixOwnerCorrespondentDto
    {
        public Guid? CorrespondentId { get; set; }

        public string? OwnerId { get; set;}
    }
}
