namespace Application.Dtos.UISettingsDTO
{
    public class SettingsDTO
    {
        public bool TourComplete { get; set; }
        public int DocumentListSize { get; set; }
        public DarkModeDTO? Dark_mode { get; set; }
        public bool NotesEnabled { get; set; }
        public Dictionary<string, object>? SavedViews { get; set; }
        public PermissionsDefaultDTO? Permissions { get; set; }
        public string Language { get; set; }
        public UpdateCheckingDTO? UpdateChecking { get; set; }
    }
}