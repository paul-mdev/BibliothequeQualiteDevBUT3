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
        // === Clés primaires ===
        modelBuilder.Entity<BookModel>()
            .HasKey(b => b.book_id);

        modelBuilder.Entity<UsersModel>()
            .HasKey(u => u.user_id);

        modelBuilder.Entity<RolesModel>()
            .HasKey(r => r.role_id);

        modelBuilder.Entity<RightsModel>()
            .HasKey(r => r.right_id);

        modelBuilder.Entity<RoleRightsModel>()
            .HasKey(rr => new { rr.role_id, rr.right_id });

        modelBuilder.Entity<DelayModel>()
            .HasKey(d => d.delay_id);

        modelBuilder.Entity<LibraryStockModel>()
            .HasKey(s => s.book_id);

        modelBuilder.Entity<BorrowedModel>()
            .HasKey(b => b.id_borrow);

        // === Relations RoleRights ===
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

        // === Relation Users → Role ===
        modelBuilder.Entity<UsersModel>()
            .HasOne(u => u.role)  // propriété avec majuscule
            .WithMany(r => r.users)
            .HasForeignKey(u => u.role_id)
            .OnDelete(DeleteBehavior.Restrict);

        // === Relation LIBRARY_STOCK 1:1 avec BOOK ===
        modelBuilder.Entity<LibraryStockModel>()
            .HasOne<BookModel>()
            .WithOne()
            .HasForeignKey<LibraryStockModel>(s => s.book_id)
            .OnDelete(DeleteBehavior.Cascade);

        // === Relations BORROWED (CRITIQUES : configuration explicite pour éviter shadow properties) ===
        modelBuilder.Entity<BorrowedModel>(entity =>
        {
            // Relation avec l'utilisateur
            entity.HasOne(b => b.User)
                  .WithMany(u => u.BorrowedBooks)  // collection inverse recommandée
                  .HasForeignKey(b => b.user_id)
                  .OnDelete(DeleteBehavior.Cascade);

            // Relation avec le livre
            entity.HasOne(b => b.Book)
                  .WithMany()
                  .HasForeignKey(b => b.book_id)
                  .OnDelete(DeleteBehavior.Cascade);

            // Contrainte : un seul emprunt actif par utilisateur et par livre
            entity.HasIndex(b => new { b.user_id, b.book_id })
                  .IsUnique()
                  .HasFilter("[is_returned] = 0");
        });

        base.OnModelCreating(modelBuilder);
    }
}