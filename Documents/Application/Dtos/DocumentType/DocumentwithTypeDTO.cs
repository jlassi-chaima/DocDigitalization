using Application.Dtos.Documents;


namespace Application.Dtos.DocumentType
{
    public  class DocumentwithTypeDTO
    {
        public Guid DocumentId { get; set; }
        public DocumentDto Document { get; set; }
        public Guid DocumentTypeId { get; set; }
        public DocumentTypeDetailsDTO DocumentType { get; set; }
    }
}
