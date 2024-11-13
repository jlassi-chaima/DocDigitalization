using Core.Domain;

namespace Domain.MailAccounts
{
    public class MailAccount : BaseEntity
    {
        public string Name { get; set; }
        public string IMAP_Server { get; set; }
        public int IMAP_Port { get; set; }
        public Imap_Security IMAP_Security { get; set; }

        public string Username { get; set; }
        public Guid? GroupId { get; set; }

        public string Password { get; set; }

        public bool Is_token { get; set; }
        public string Character_set { get; set; }
    }
}
