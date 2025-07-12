using Microsoft.EntityFrameworkCore;
using WadesWineWatcher.Models;
using backend.Models;

namespace wwwbackend.data
{
    public class WineDbContext : DbContext
    {
        public WineDbContext(DbContextOptions<WineDbContext> options) : base(options) { }

        public DbSet<Wine> Wines { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
