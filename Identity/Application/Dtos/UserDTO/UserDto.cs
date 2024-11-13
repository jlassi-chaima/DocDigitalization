using Domain.User;

namespace Application.Dtos.UserDTO
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public bool? Superuser_status { get; set; }
        public ICollection<UserGroups> Groups { get; set; }
        public bool? Active { get; set; } = true;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();

        public IList<string>? Permissions { get; set; }
    }

    public class DarkModeSettingsDto
    {
        public bool Use_system { get; set; }
        public bool Enabled { get; set; }
        public bool Thumb_inverted { get; set; }
    }

    public class PermissionsDto
    {
        public Guid Default_owner { get; set; }
        public ICollection<Guid> Default_view_users { get; set; }
        public ICollection<Guid> Default_view_groups { get; set; }
        public ICollection<Guid> Default_edit_users { get; set; }
        public ICollection<Guid> Default_edit_groups { get; set; }
    }

    public class SettingsDto
    {
        public bool Tour_complete { get; set; }
        public int DocumentListSize { get; set; }
        public DarkModeSettingsDto Dark_mode { get; set; }
        public bool Notes_enabled { get; set; }
        public object Saved_views { get; set; } = new { };
        public PermissionsDto Permissions { get; set; }
        public string Language { get; set; }
        public string? Settings { get; set; }
    }

    public class ResultDto
    {
        public UserDto User { get; set; }
        public SettingsDto? Settings { get; set; }
        public List<string> Permissions { get; set; }
    }
}
