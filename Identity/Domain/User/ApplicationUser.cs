using Core.Domain;
using Microsoft.AspNetCore.Identity;
namespace Domain.User
{
    public class ApplicationUser : IdentityUser
    {
        public bool Superuser_status { get; set; }

        public bool? Active { get; set; } = true;

        public string? FirstName { get; set; }
        public string? LastName  { get; set; }
        public ICollection<UserGroups>? Groups { get; set; } = new List<UserGroups>();
        public IList<string>? Permissions { get; set; } =new List<string>();    

        // Static creation method
        public static ApplicationUser Create(string email, bool superuserStatus,bool active,
                                  ICollection<UserGroups>? groups = null, IList<string>? permissions = null)
        {
            return new ApplicationUser
            {
                UserName = email,
                NormalizedUserName =email.ToUpper(),
                NormalizedEmail = email.ToUpper(),
                Email = email,
                Active = active,
                Superuser_status = superuserStatus,
                Groups = groups,
                Permissions = permissions
            };
        }
    }
}
