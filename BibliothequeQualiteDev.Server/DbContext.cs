using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<BookModel> BOOK => Set<BookModel>();
    public DbSet<UserModel> USER => Set<UserModel>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BookModel>()
            .HasKey(b => b.book_id); // Explicit primary key

        modelBuilder.Entity<UserModel>()
        .HasKey(u => u.user_id); // Explicit primary key


        base.OnModelCreating(modelBuilder);
    }

}

