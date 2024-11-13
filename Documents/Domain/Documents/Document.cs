using Core.Domain;
using Domain.DocumentManagement.Correspondent;
using Domain.DocumentManagement.CustomFields;
using Domain.DocumentManagement.DocumentNote;
using Domain.DocumentManagement.DocumentTypes;
using Domain.DocumentManagement.StoragePath;
using Domain.DocumentManagement.tags;
using Domain.Templates.Enum;
namespace Domain.Documents
{

    public class Document : BaseEntity
    {
        public string Title { get; set; }
       
        public string Content { get; set; }
        public string Base64Data  { get; set; }

        public string? Archive_Serial_Number{ get; set; }
        public string FileData { get; set; }
        public string? Owner { get; set; }
        public  Guid? GroupId { get; set; }

        public DateTime? Created { get; set; }
        public string? Lang { get; set; }
        public string Checksum { get; set; }
        public string MimeType { get; set; }
        public List<DocumentTags>? Tags { get; set; }
        public Guid? DocumentTypeId{ get; set; }
        public DocumentType? Document_Type { get; set; }
        public Guid? CorrespondentId { get; set; }
        public Correspondent? Correspondent { get; set; }

        public ICollection<DocumentCustomField>? DocumentsCustomFields { get; set; } =new List<DocumentCustomField>();

        public string ThumbnailUrl { get; set; }
        public Guid? StoragePathId { get; set; }
        public StoragePath? StoragePath { get; set; }

        public string? Mailrule { get; set; }

        public List<DocumentNote> Notes { get; set; }
        public DocumentSource Source { get; set; }

        public List<string>? UsersView { get; set; }
        public List<string>? GroupsView { get; set; }

        public List<string>? UsersChange { get; set; }  
        public List<string>? GroupsChange { get; set; }
        public byte[] Key {  get; set; }
        public byte[] Iv { get; set; }
        public static Document Create(
            string title,
            string asn,
            string content ,
            string fileData,
            string owner,
            string mimetype,
            string checksum
           )
        {
            Document doc = new()
            {
                Title = title,
                Archive_Serial_Number = asn,
                Content = content,
                FileData = fileData,
                Owner = owner,
                MimeType = mimetype,
                Checksum = checksum
                
            };

            var @event = new DocumentCreatedEvent(doc.Id, doc.Title);
            doc.AddDomainEvent(@event);

            return doc;
        }
        public static Document Upload(
            string title,
            string fileData,
            string content,
            string asn,
            string owner
    
           )
        {
            Document doc = new()
            {

                Title = title,
                FileData = fileData,
                Content = content,
                Archive_Serial_Number = asn,
                Owner = owner,
     

            };

            var @event = new DocumentCreatedEvent(doc.Id, doc.Title);
            doc.AddDomainEvent(@event);

            return doc;
        }

    }
   

    
}