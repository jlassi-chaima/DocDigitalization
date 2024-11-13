
using Core.Domain;
using Domain.Documents;


namespace Domain.DocumentManagement.DocumentNote
{
    public class DocumentNote : BaseEntity
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Note { get; set; }
        public string User { get; set; }

        public Guid DocumentId { get; set; }
        public virtual Document Document { get; set; }

        public static DocumentNote Create(
             string note, string user)
        {
            DocumentNote documentNote = new()
            {

                Note = note,
                User = user
            };
            return documentNote;
        }

    }
}
