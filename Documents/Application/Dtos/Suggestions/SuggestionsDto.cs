

namespace Application.Dtos.Suggestions
{
    public class SuggestionsDto
    {
        public List<Guid> Tags { get; set; }
        public List<Guid> Correspondents { get; set; }
        public List<Guid> StoragePaths { get; set; }
        public List<Guid> DocumentTypes { get; set; }
        public List<Guid> Dates { get; set; }
    }
}
