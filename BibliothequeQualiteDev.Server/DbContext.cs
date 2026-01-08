using Microsoft.EntityFrameworkCore;
using BibliothequeQualiteDev.Server.Models;

// Contexte EF Core principal de l'application
// Centralise le mapping entre les entités C# et la base MySQL
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    // ===============================
    // DbSet : tables exposées
    // ===============================
    public DbSet<BookModel> BOOK => Set<BookModel>();
    public DbSet<UsersModel> USERS => Set<UsersModel>();
    public DbSet<RolesModel> ROLES => Set<RolesModel>();
    public DbSet<BorrowedModel> BORROWED => Set<BorrowedModel>();
    public DbSet<LibraryStockModel> LIBRARY_STOCK => Set<LibraryStockModel>();
    public DbSet<RightsModel> RIGHTS => Set<RightsModel>();
    public DbSet<RoleRightsModel> ROLE_RIGHTS => Set<RoleRightsModel>();
    public DbSet<DelayModel> DELAY => Set<DelayModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        // Définition des clés primaires
        modelBuilder.Entity<BookModel>().HasKey(b => b.book_id);
        modelBuilder.Entity<UsersModel>().HasKey(u => u.user_id);
        modelBuilder.Entity<RolesModel>().HasKey(r => r.role_id);
        modelBuilder.Entity<RightsModel>().HasKey(r => r.right_id);
        modelBuilder.Entity<RoleRightsModel>()
            .HasKey(rr => new { rr.role_id, rr.right_id });
        modelBuilder.Entity<DelayModel>().HasKey(d => d.delay_id);
        modelBuilder.Entity<LibraryStockModel>().HasKey(s => s.book_id);
        modelBuilder.Entity<BorrowedModel>().HasKey(b => b.id_borrow);

        // Relations Role
        modelBuilder.Entity<RoleRightsModel>()
            .HasOne(rr => rr.role)
            .WithMany(r => r.role_rights)
            .HasForeignKey(rr => rr.role_id)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RoleRightsModel>()
            .HasOne(rr => rr.right)
            .WithMany(r => r.role_rights)
            .HasForeignKey(rr => rr.right_id)
            .OnDelete(DeleteBehavior.Cascade);

        // Relation User
        modelBuilder.Entity<UsersModel>()
            .HasOne(u => u.role)
            .WithMany(r => r.users)
            .HasForeignKey(u => u.role_id)
            .OnDelete(DeleteBehavior.Restrict);
        // Restrict : empêche la suppression d’un rôle encore utilisé


        // Relation LibraryStock
        modelBuilder.Entity<LibraryStockModel>()
            .HasOne<BookModel>()
            .WithOne()
            .HasForeignKey<LibraryStockModel>(s => s.book_id)
            .OnDelete(DeleteBehavior.Cascade);
        // La suppression d’un livre supprime son stock associé


        // Relations Borrowed (emprunts)
        modelBuilder.Entity<BorrowedModel>(entity =>
        {
            // Lien emprunt -> utilisateur
            entity.HasOne(b => b.User)
                .WithMany(u => u.BorrowedBooks)
                .HasForeignKey(b => b.user_id)
                .OnDelete(DeleteBehavior.Cascade);

            // Lien emprunt -> livre
            entity.HasOne(b => b.Book)
                .WithMany(book => book.Borrowed)
                .HasForeignKey(b => b.book_id)
                .OnDelete(DeleteBehavior.Cascade);
            // La suppression d’un livre ou d’un utilisateur
            // supprime les emprunts associés

            // Contrainte métier :
            // un utilisateur ne peut avoir qu’un seul emprunt actif
            // pour un même livre
            entity.HasIndex(b => new { b.user_id, b.book_id })
                .IsUnique()
                .HasFilter("[is_returned] = 0");
        });

        base.OnModelCreating(modelBuilder);
    }
}