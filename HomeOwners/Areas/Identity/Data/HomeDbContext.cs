using HomeOwners.Models;
using HomeOwners.Models.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HomeOwners.Areas.Identity.Data
{
    public class HomeDbContext : IdentityDbContext<
        IdentityUser, // Base user type
        IdentityRole,
        string,
        IdentityUserClaim<string>,
        IdentityUserRole<string>,
        IdentityUserLogin<string>,
        IdentityRoleClaim<string>,
        IdentityUserToken<string>>
    {
        public HomeDbContext(DbContextOptions<HomeDbContext> options)
            : base(options)
        {
        }

        public DbSet<AdminUser> AdminUsers { get; set; }
        public DbSet<StaffUser> StaffUsers { get; set; }
        public DbSet<HomeOwnerUser> HomeOwnerUsers { get; set; }
        public DbSet<UserPreferences> UserPreferences { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Poll> Polls { get; set; }
        public DbSet<PollOption> PollOptions { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure table names
            builder.Entity<AdminUser>().ToTable("AdminUsers");
            builder.Entity<StaffUser>().ToTable("StaffUsers");
            builder.Entity<HomeOwnerUser>().ToTable("HomeOwnerUsers");

            builder.Entity<UserPreferences>()
                .HasOne(p => p.User)
                .WithOne(u => u.Preferences)
                .HasForeignKey<UserPreferences>(p => p.UserId);


            // Configure Poll relationships
            builder.Entity<Poll>()
                .HasMany(p => p.Options)
                .WithOne(o => o.Poll)
                .HasForeignKey(o => o.PollId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PollOption>()
                .Property(p => p.Votes)
                .HasDefaultValue(0);
        }
    }
}
