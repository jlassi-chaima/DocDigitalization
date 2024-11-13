
using Domain.Templates.Enum;
using Microsoft.AspNetCore.Http;

namespace Application.Dtos.Documents
{
    public class DocumentAPI
    {
        public string Title { get; set; }


        public string Content { get; set; }

        public string? Archive_Serial_Number { get; set; }
        public string FileData { get; set; }
        public string? Owner { get; set; }
        public DocumentSource Source { get; set; }
        public string? Mailrule { get; set; }
        public string Checksum { get; set; }
        public string MimeType { get; set; }
        public List<Guid>? Tags { get; set; }
        public Guid? DocumentTypeId { get; set; }
        public Guid? CorrespondentId { get; set; }
    }
}
