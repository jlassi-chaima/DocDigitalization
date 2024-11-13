using Domain.MailAccounts;
using Domain.MailRules;
using Microsoft.EntityFrameworkCore;


namespace Infrastructure.Persistence
{
    public class DBContext : DbContext
    {
        public DBContext(DbContextOptions options) : base(options)
        {
        }
       public DbSet<MailAccount> MailAccounts { get; set; }
       public DbSet<MailRule> MailRules { get; set; }

    }
}
