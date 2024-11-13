
using Application.Dtos.PermissionsDTO;
using Domain.User;

namespace Application.Dtos.UserDTO
{
    public class ListUsersDto
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public bool SuperuserStatus { get; set; }
        public bool Active { get; set; }
        public string? First_name { get; set; }
        public string? Last_name { get; set; }

        public string? Email { get; set; }
        public string? PasswordHash { get; set; }
        
        public ICollection<UserGroups>? Groups { get; set; }
        public IList<string>? Permissions { get; set; }
    }
}
