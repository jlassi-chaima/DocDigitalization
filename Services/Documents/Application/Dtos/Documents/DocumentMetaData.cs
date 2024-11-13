using Domain.Templates.Enum;

namespace Application.Dtos.Documents
{
    public class DocumentMetadata
    {
        public string? MediaFilename { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DateModified { get; set; }
        public string? Checksum { get; set; }
        public long? OriginalFileSize { get; set; }
        public string? MimeType { get; set; }
        public string? Lang { get; set; }
        public string? MailRule { get; set; }
        public DocumentSource? Source { get; set; }
    }
}
