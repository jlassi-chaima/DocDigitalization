namespace Application.Dtos.UISettingsDTO
{
    public class PermissionsDefaultDTO
    {
        public string? DefaultOwner { get; set; }
        public List<string>? DefaultViewUsers { get; set; }
        public List<string>? DefaultViewGroups { get; set; }
        public List<string>? DefaultEditUsers { get; set; }
        public List<string>? DefaultEditGroups { get; set; }
    }
}