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
        }
    }
}
