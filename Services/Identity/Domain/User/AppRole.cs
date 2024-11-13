using Microsoft.AspNetCore.Identity;

namespace Domain.User
{
    public class AppRole : IdentityRole
    {
        public ICollection<AppUserRole> UserRoles { get; set; }

    }
}
