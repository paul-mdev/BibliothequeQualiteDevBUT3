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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configuration des clés primaires
        modelBuilder.Entity<BookModel>()
            .HasKey(b => b.book_id);

        modelBuilder.Entity<UsersModel>()
            .HasKey(u => u.user_id);

        modelBuilder.Entity<BorrowedModel>()
            .HasKey(b => b.id_borrow);

        modelBuilder.Entity<LibraryStockModel>()
            .HasKey(ls => ls.stock_id);

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

        base.OnModelCreating(modelBuilder);
    }
}

