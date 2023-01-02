using Microsoft.EntityFrameworkCore;
using BookSeller.Models;

namespace BookSeller.DataAccess
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        { }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Cover> Covers { get; set; }

    }
}
