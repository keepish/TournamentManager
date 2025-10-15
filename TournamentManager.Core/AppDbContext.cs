using Microsoft.EntityFrameworkCore;
using TournamentManager.Core.Models;

namespace TournamentManager.Core
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Tournament> Tournaments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Tournament>()
                .HasOne(t => t.Organizer)
                .WithMany(u => u.OrganizedTournaments)
                .HasForeignKey(t => t.OrganizerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Организатор" },
                new Role { Id = 2, Name = "Судья" },
                new Role { Id = 3, Name = "Участник" }
            );
        }
    }
}
