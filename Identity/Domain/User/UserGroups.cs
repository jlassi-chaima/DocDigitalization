namespace Domain.User
{
    public class UserGroups
    {
        public string  UserID { get; set; }

        public virtual ApplicationUser User { get; set; }

        public Guid GroupID { get; set; }

        public virtual Groups Group { get; set; }


    }
}