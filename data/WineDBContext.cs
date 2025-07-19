using Microsoft.EntityFrameworkCore;
using WadesWineWatcher.Models;

namespace wwwbackend.data
{
    public class WineDbContext : DbContext
    {
        public WineDbContext(DbContextOptions<WineDbContext> options) : base(options) { }

        public DbSet<Wine> Wines { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<WineUser> WineUsers { get; set; }
        public DbSet<Event> Events { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WineUser>()
                .HasKey(wu => new { wu.WineId, wu.UserId });

            modelBuilder.Entity<WineUser>()
                .HasOne(wu => wu.Wine)
                .WithMany(w => w.WineUsers)
                .HasForeignKey(wu => wu.WineId);

            modelBuilder.Entity<WineUser>()
                .HasOne(wu => wu.User)
                .WithMany(u => u.WineUsers)
                .HasForeignKey(wu => wu.UserId);

            // Event to Wine (many events to one wine)
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Wine)
                .WithMany(w => w.Events)
                .HasForeignKey(e => e.WineId)
                .OnDelete(DeleteBehavior.Cascade);  // Optional: delete events if wine deleted
                
            // Event to User (many events to one user as creator)
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Creator)
                .WithMany(u => u.EventsCreated)
                .HasForeignKey(e => e.CreatorId)
                .OnDelete(DeleteBehavior.Restrict);  // Prevent deleting user if events exist (optional)
        }
    }
}
