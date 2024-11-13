
namespace Application.Dtos.SelectionData
{
    public class SelectionDataDTO
    {
        public List<DetailsDTO>? Selected_correspondents { get; set; }
        public List<DetailsDTO>? Selected_tags { get; set; }
        public List<DetailsDTO>? Selected_Document_types { get; set; }
        public List<DetailsDTO>? Selected_storage_paths { get; set; }
    }
}
