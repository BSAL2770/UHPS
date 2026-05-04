using Microsoft.EntityFrameworkCore;

namespace UHPS.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // DbSet<T> properties will go here on Day 2 when we define models.
}