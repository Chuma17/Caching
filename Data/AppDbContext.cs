using Caching.Models;
using Microsoft.EntityFrameworkCore;

namespace Caching.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            
        }

        public DbSet<Driver> Drivers { get; set; }
    }
}
