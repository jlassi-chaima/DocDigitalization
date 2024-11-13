using Domain.DocumentManagement.Correspondent;
using Domain.DocumentManagement.DocumentTypes;
using Domain.DocumentManagement.tags;

namespace Application.Dtos.Documents
{
    public class DocumentDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }

        public string Archive_Serial_Number { get; set; }
        public string FileData { get; set; }
        public string? Owner { get; set; }

        public string Checksum { get; set; }
        public string MimeType { get; set; }
        public List<DocumentTags>? DocumentTags { get; set; }
        public List<DocumentwithTypes>? DocumentWithTypes { get; set; }
    }
}
