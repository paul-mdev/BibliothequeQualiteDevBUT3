using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<BookModel> BOOK => Set<BookModel>();
    public DbSet<UsersModel> USERS => Set<UsersModel>();
    public DbSet<RolesModel> ROLES => Set<RolesModel>();
    public DbSet<BorrowedModel> BORROWED => Set<BorrowedModel>();
    public DbSet<LibraryStockModel> LIBRARY_STOCK => Set<LibraryStockModel>();
    public DbSet<DelayModel> DELAY => Set<DelayModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Clés primaires explicites
        modelBuilder.Entity<BookModel>()
            .HasKey(b => b.book_id);

        modelBuilder.Entity<UsersModel>()
            .HasKey(u => u.user_id);

        modelBuilder.Entity<BorrowedModel>()
            .HasKey(b => b.id_borrow);

        // Nouvelle clé primaire : book_id (une seule ligne par livre)
        modelBuilder.Entity<LibraryStockModel>()
            .HasKey(s => s.book_id);

        modelBuilder.Entity<RolesModel>()
            .HasKey(r => r.role_id);

        modelBuilder.Entity<DelayModel>()
            .HasKey(d => d.delay_id);

        // Relation : LIBRARY_STOCK 1:1 avec BOOK
        modelBuilder.Entity<LibraryStockModel>()
            .HasOne<BookModel>()
            .WithOne()
            .HasForeignKey<LibraryStockModel>(s => s.book_id)
            .OnDelete(DeleteBehavior.Cascade);

        // Relation : BORROWED → BOOK (directement via book_id)
        modelBuilder.Entity<BorrowedModel>()
            .HasOne<BookModel>()
            .WithMany()
            .HasForeignKey(b => b.book_id);

        // Contrainte d'unicité : un utilisateur ne peut avoir qu'un emprunt actif par livre
        modelBuilder.Entity<BorrowedModel>()
            .HasIndex(b => new { b.user_id, b.book_id, b.is_returned })
            .IsUnique()
            .HasFilter("[is_returned] = 0"); // Seulement pour les non retournés

        base.OnModelCreating(modelBuilder);
    }
}