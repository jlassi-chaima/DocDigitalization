using Core.Domain;


namespace Domain.FileShare
{
    public class ShareFolder : BaseEntity
    {
        public string FolderPath { get; set; }
        public string ShareName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string? Owner { get; set; }
        public Guid? GroupId { get; set; }

        public DateTime CreationTime { get; set; }


    }
}
