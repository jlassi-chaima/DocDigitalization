

namespace Application.Dtos.DocumentNote
{
    public class DocumentNoteDto
    {
        public DateTime Created { get; set; } = DateTime.Now;
        public string Note { get; set; }
       public string User { get; set; } = string.Empty;
        public Guid Id { get; set; }
    }
}
