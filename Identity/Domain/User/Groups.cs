using Core.Domain;

namespace Domain.User
{
    public class Groups : BaseEntity
    {
        public string Name { get; set; }

        public ICollection<UserGroups>? Users { get; set; }

        public IList<string>? Permissions { get; set; }= new List<string>(); 

        public static Groups Create(string name, IList<string>? permissions = null)
        {
            return new Groups
            {
                
                Name = name,
                Permissions = permissions
            };
        }
    }
}