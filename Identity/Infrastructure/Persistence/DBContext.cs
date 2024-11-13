using Domain.Settings;
using Domain.User;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;


namespace Infrastructure.Persistence
{
    public class DBContext : IdentityDbContext<ApplicationUser>
    {
        public DBContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<Groups> Groups { get; set; }

        public DbSet<UserGroups> UserGroups { get; set; }
        public DbSet<UISettings > UISettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //Document Tags
            modelBuilder.Entity<UserGroups>(x => x.HasKey(x => new {x.GroupID , x.UserID }));
            modelBuilder.Entity<UserGroups>().HasOne(dt => dt.User).WithMany(t => t.Groups).HasForeignKey(dt => dt.UserID);
            modelBuilder.Entity<UserGroups>().HasOne(dt => dt.Group).WithMany(d => d.Users).HasForeignKey(dt => dt.GroupID);
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable("AspNetUsers"); // Ensure the table name is correct
            });
            // Configure UISettings
            modelBuilder.Entity<UISettings>()
                .HasKey(us => us.Id); // Assuming UISettings has a primary key Id

            modelBuilder.Entity<UISettings>()
                     .HasOne(us => us.User)
                     .WithOne() // No need for WithOne because we defined the navigation property on UISettings
                     .HasForeignKey<UISettings>(us => us.UserId)
                     .OnDelete(DeleteBehavior.Cascade); // Cascade delete when User is deleted

        }
    }
}
