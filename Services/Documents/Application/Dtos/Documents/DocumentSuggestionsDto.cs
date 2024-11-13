using Application.Dtos.CustomField;
using Application.Dtos.DocumentNote;
using Application.Dtos.Tag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Documents
{
    public class DocumentSuggestionsDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        //public string FileData { get; set; }
        public string Owner { get; set; }
        public string Archive_serial_number { get; set; }
        public DateTime Created { get; set; }
        public Guid DocumentTypeId { get; set; }

        //public Guid? CorrespondentId { get; set; }
        public Guid CorrespondentId { get; set; }
        public Guid StoragePathId { get; set; }
        public List<DocumentTagsDTO>? Tags { get; set; }


        public List<DocumentCustomFieldDTO>? DocumentsCustomFields { get; set; }

        public List<DocumentNoteDto>? DocumentNotes { get; set; }
    }
}
