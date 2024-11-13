

namespace Application.Dtos.ShareFolder
{
    public class ShareFolderDto
    {
        public string FolderPath { get; set; }
        public string ShareName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string? Owner { get; set; }

    }
}
