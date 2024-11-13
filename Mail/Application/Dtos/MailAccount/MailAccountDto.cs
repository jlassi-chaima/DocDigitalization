using Domain.MailAccounts;


namespace Application.Dtos.MailAccount
{
    public class MailAccountDto
    {
        public string Name { get; set; }
        public string IMAP_Server { get; set; }
        public int IMAP_Port { get; set; }
        public Imap_Security IMAP_Security { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }
        public string? Owner { get; set; }

        public string Character_set { get; set; }
    }
}
