using Application.Dtos.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.SelectionData
{
    public class SelectionDataDocuments
    {
        public List<string> Documents { get; set; }
    }
    public class DocumentDTO
    {
        public Guid Id { get; set; }
        // Other properties as needed
    }
}
