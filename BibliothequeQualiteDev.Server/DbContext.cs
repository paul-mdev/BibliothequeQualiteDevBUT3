using Microsoft.EntityFrameworkCore;
using BibliothequeQualiteDev.Server.Models;

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
    public DbSet<RightsModel> RIGHTS => Set<RightsModel>();
    public DbSet<RoleRightsModel> ROLE_RIGHTS => Set<RoleRightsModel>();
    public DbSet<DelayModel> DELAY => Set<DelayModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configuration des clés primaires
        modelBuilder.Entity<BookModel>()
            .HasKey(b => b.book_id);

        modelBuilder.Entity<UsersModel>()
            .HasKey(u => u.user_id);



        modelBuilder.Entity<RolesModel>()
            .HasKey(r => r.role_id);

        // ⭐ Configuration de RightsModel
        modelBuilder.Entity<RightsModel>()
            .HasKey(r => r.right_id);

        // ⭐ Configuration de RoleRightsModel (table de liaison avec clé composite)
        modelBuilder.Entity<RoleRightsModel>()
            .HasKey(rr => new { rr.role_id, rr.right_id });

        // ⭐ Configuration des relations pour RoleRightsModel
        modelBuilder.Entity<RoleRightsModel>()
            .HasOne(rr => rr.role)
            .WithMany(r => r.role_rights)
            .HasForeignKey(rr => rr.role_id);

        modelBuilder.Entity<RoleRightsModel>()
            .HasOne(rr => rr.right)
            .WithMany(r => r.role_rights)
            .HasForeignKey(rr => rr.right_id);

        // ⭐ Configuration de la relation UsersModel -> RolesModel
        modelBuilder.Entity<UsersModel>()
            .HasOne(u => u.role)
            .WithMany(r => r.users)
            .HasForeignKey(u => u.role_id)
            .OnDelete(DeleteBehavior.Restrict); // Évite la suppression en cascade

        modelBuilder.Entity<DelayModel>()
            .HasKey(d => d.delay_id);

        modelBuilder.Entity<LibraryStockModel>()
            .HasKey(d => d.book_id);

        // Relation : LIBRARY_STOCK 1:1 avec BOOK
        modelBuilder.Entity<LibraryStockModel>()
            .HasOne<BookModel>()
            .WithOne()
            .HasForeignKey<LibraryStockModel>(s => s.book_id)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BorrowedModel>()
            .HasKey(b => b.id_borrow);


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