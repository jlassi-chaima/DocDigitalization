using Microsoft.AspNetCore.Identity;

namespace Domain.User
{
    public class AppUserRole : IdentityUserRole<string>
    {
 
      
        public required ApplicationUser User { get; set; }
        public required AppRole Role { get; set; }
    }
}
