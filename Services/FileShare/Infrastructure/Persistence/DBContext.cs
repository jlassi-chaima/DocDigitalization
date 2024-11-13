using Domain.FileShare;
using Microsoft.EntityFrameworkCore;


namespace Infrastructure.Persistence
{
    public class DBContext : DbContext
    {
        public DBContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<ShareFolder> FileShares { get; set; }
    }
}
