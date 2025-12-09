using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<BookModel> BOOK => Set<BookModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BookModel>()
            .HasKey(b => b.book_id); // Explicit primary key

        base.OnModelCreating(modelBuilder);
    }

}

