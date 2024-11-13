namespace Application.Dtos.UISettingsDTO
{
    public class UISettingsDTO
    {
        public bool Tour_complete { get; set; }
        public int DocumentListSize { get; set; }
        public bool DarkMode_use_system { get; set; }

        public bool DarkMode_enabled { get; set; }
        public bool DarkMode_thumb_inverted { get; set; }
        public bool Notes_enabled { get; set; }
        public string Language { get; set; }
        public IList<string> Default_view_users { get; set; } = new List<string>();
        public List<Guid> Default_view_groups { get; set; } = new List<Guid>();
        public IList<string> Default_edit_users { get; set; } = new List<string>();
        public List<Guid> Default_edit_groups { get; set; } = new List<Guid>();
        public string User { get; set; }

    }
}
