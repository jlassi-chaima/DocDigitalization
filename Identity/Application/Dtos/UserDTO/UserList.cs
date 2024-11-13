using Domain.User;

namespace Application.Dtos.UserDTO
{
    public class UserList
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool IsSuperUser { get; set; }
        public List<string> Groups { get; set; } = new List<string>();
        public IList<string>? Permissions { get; set; }
        public string PasswordHash { get; set; }
        public string? ConfirmPassword { get; set; }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool isActive { get; set; } = true;
    }
}
