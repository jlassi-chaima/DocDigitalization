using Domain.MailAccounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.MailAccount
{
    public class MailAccountId
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
