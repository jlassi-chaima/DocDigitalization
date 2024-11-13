using Core.Domain;

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
       

    }
   

    
}