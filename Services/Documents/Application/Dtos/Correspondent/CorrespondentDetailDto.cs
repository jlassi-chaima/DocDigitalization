using Domain.DocumentManagement;


namespace Application.Dtos.Correspondent
{
    public class CorrespondentDetailDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<string> Match { get; set; }
        public Matching_Algorithms matching_algorithm { get; set; }
        public bool Is_senstive { get; set; }
        public DateTime? Last_correspondence { get; set; }
        public string Owner { get; set; }
        public int Document_count { get; set; }
    }
}
