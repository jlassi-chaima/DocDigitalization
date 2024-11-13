namespace Application.Dtos.Permission
{
    public class UserGroupPermission
    {
        public List<string>? Users { get; set; } = new List<string>();
        public List<string>? Groups { get; set; } = new List<string>();
    }
}