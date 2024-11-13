
namespace Application.Consumers.RestAPIDocuments.Dtos
{
    public class DocumentTags
    {
        public Guid DocumentId { get; set; }

        public Document Document { get; set; }
        public Guid TagId { get; set; }
        public TagsDto Tag { get; set; }
    }
}
