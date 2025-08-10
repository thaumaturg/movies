using Microsoft.EntityFrameworkCore;

namespace Movies.API.Data;

public class MoviesDbContext : DbContext
{
    public MoviesDbContext(DbContextOptions dbContextOptions) : base(dbContextOptions)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
