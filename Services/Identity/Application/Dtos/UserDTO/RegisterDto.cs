using Domain.User;
using Newtonsoft.Json;

namespace Application.Dtos.UserDTO
{
    public class RegisterDto
    {

        public  string? Id { get; set; }
        public required string UserName { get; set; }
        public required string Email { get; set; }
        //[JsonProperty("passwordHash")]
        public  string? PasswordHash { get; set; }

        public bool IsSuperUser { get; set; } = false;


        public bool IsActive { get; set; } = true;

        public string? FirstName { get; set; }

        public string? LastName { get; set; }
        public ICollection<Guid>? Groups { get; set; }

        public IList<string>? Permissions { get; set; }=new List<string>(); 

    }
}
