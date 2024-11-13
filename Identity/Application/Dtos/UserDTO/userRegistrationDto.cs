using Application.Dtos.GroupDTO;
using Newtonsoft.Json.Converters;
using Domain.User;
using Newtonsoft.Json;

namespace Application.Dtos.User
{
    public class UserRegistrationDto
    {
     
            public string UserName { get; set; }
      
            public string FirstName { get; set; }

            public string LastName { get; set; }
            public string Email { get; set; }
            public bool Is_superuser { get; set; }
            public bool Active { get; set; }
            public ICollection<Guid>? groups { get; set; }
            public IList<string>? User_permissions { get; set; }
            public string passwordHash { get; set; }

         
        
    }
}
