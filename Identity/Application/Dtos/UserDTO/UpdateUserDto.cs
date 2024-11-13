using Domain.User;


namespace Application.Dtos.UserDTO
{
    public class UpdateUserDto
    {

        public string UserName { get; set; }
        public required string Email { get; set; }
        public bool IsSuperUser { get; set; }
        public ICollection<Guid>? Groups { get; set; }
        public IList<string>? Permissions { get; set; }
        public string PasswordHash { get; set; }
        public string? ConfirmPassword { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
