using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<BookModel> BOOK => Set<BookModel>();
    public DbSet<UsersModel> USER => Set<UsersModel>();
    public DbSet<BorrowedModel> BORROWED => Set<BorrowedModel>();
    public DbSet<LibraryStockModel> LIBRARY_STOCK => Set<LibraryStockModel>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BookModel>()
            .HasKey(b => b.book_id); // Explicit primary key

        modelBuilder.Entity<UsersModel>()
            .HasKey(u => u.user_id); // Explicit primary key

        modelBuilder.Entity<BorrowedModel>()
            .HasKey(b => b.id_borrow); // Explicit primary key

        modelBuilder.Entity<LibraryStockModel>()
            .HasKey(b => b.stock_id); // Explicit primary key

        base.OnModelCreating(modelBuilder);
    }

}

