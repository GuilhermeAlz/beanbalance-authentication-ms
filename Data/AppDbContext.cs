using Microsoft.EntityFrameworkCore;
using AuthMicroservice.Models;

namespace AuthMicroservice.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();

            entity.HasIndex(u => u.Username).IsUnique();
        });
    }
}
