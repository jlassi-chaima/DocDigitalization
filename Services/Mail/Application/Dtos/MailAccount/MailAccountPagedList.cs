

using Domain.MailAccounts;

namespace Application.Dtos.MailAccount
{
    public class MailAccountPagedList
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string imap_server { get; set; }
        public int imap_port { get; set; }
        public Imap_Security imap_Security { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string Character_set { get; set; }
    }
}
