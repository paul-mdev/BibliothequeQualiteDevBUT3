using Microsoft.EntityFrameworkCore;
using BibliothequeQualiteDev.Server.Models;

/// <summary>
/// ===== CONTEXTE DE BASE DE DONNÉES =====
/// Gère toutes les interactions avec la base de données MySQL
/// Définit les tables (DbSet) et leurs relations
/// 
/// Tables principales :
/// - BOOK : Catalogue des livres
/// - USERS : Utilisateurs de la bibliothèque
/// - ROLES : Rôles (Admin, Professeur, Étudiant)
/// - RIGHTS : Droits d'accès (gerer_livres, etc.)
/// - BORROWED : Emprunts de livres
/// - LIBRARY_STOCK : Gestion du stock
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // ===== DÉFINITION DES TABLES (DbSet) =====
    // Chaque DbSet correspond à une table dans la base de données
    public DbSet<BookModel> BOOK => Set<BookModel>();
    public DbSet<UsersModel> USERS => Set<UsersModel>();
    public DbSet<RolesModel> ROLES => Set<RolesModel>();
    public DbSet<BorrowedModel> BORROWED => Set<BorrowedModel>();
    public DbSet<LibraryStockModel> LIBRARY_STOCK => Set<LibraryStockModel>();
    public DbSet<RightsModel> RIGHTS => Set<RightsModel>();
    public DbSet<RoleRightsModel> ROLE_RIGHTS => Set<RoleRightsModel>();
    public DbSet<DelayModel> DELAY => Set<DelayModel>();

    /// <summary>
    /// ===== CONFIGURATION DU MODÈLE =====
    /// Méthode appelée lors de la création du modèle
    /// Configure les clés primaires, relations et contraintes
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ========================================
        // DÉFINITION DES CLÉS PRIMAIRES
        // ========================================

        modelBuilder.Entity<BookModel>()
            .HasKey(b => b.book_id);

        modelBuilder.Entity<UsersModel>()
            .HasKey(u => u.user_id);

        modelBuilder.Entity<RolesModel>()
            .HasKey(r => r.role_id);

        modelBuilder.Entity<RightsModel>()
            .HasKey(r => r.right_id);

        // ===== CLÉ COMPOSITE =====
        // ROLE_RIGHTS utilise une clé primaire composite (role_id, right_id)
        modelBuilder.Entity<RoleRightsModel>()
            .HasKey(rr => new { rr.role_id, rr.right_id });

        modelBuilder.Entity<DelayModel>()
            .HasKey(d => d.delay_id);

        modelBuilder.Entity<LibraryStockModel>()
            .HasKey(s => s.book_id);

        modelBuilder.Entity<BorrowedModel>()
            .HasKey(b => b.id_borrow);

        // ========================================
        // RELATIONS ROLE_RIGHTS (Many-to-Many)
        // ========================================
        // Table de liaison entre ROLES et RIGHTS

        // Relation RoleRights → Role
        modelBuilder.Entity<RoleRightsModel>()
            .HasOne(rr => rr.role)              // Un RoleRight a un Role
            .WithMany(r => r.role_rights)       // Un Role a plusieurs RoleRights
            .HasForeignKey(rr => rr.role_id)
            .OnDelete(DeleteBehavior.Cascade);  // Suppression en cascade

        // Relation RoleRights → Right
        modelBuilder.Entity<RoleRightsModel>()
            .HasOne(rr => rr.right)             // Un RoleRight a un Right
            .WithMany(r => r.role_rights)       // Un Right a plusieurs RoleRights
            .HasForeignKey(rr => rr.right_id)
            .OnDelete(DeleteBehavior.Cascade);

        // ========================================
        // RELATION USERS → ROLES (Many-to-One)
        // ========================================
        // Chaque utilisateur a un rôle

        modelBuilder.Entity<UsersModel>()
            .HasOne(u => u.role)                // Un User a un Role
            .WithMany(r => r.users)             // Un Role a plusieurs Users
            .HasForeignKey(u => u.role_id)
            .OnDelete(DeleteBehavior.Restrict); // Empêche la suppression d'un rôle utilisé

        // ========================================
        // RELATION LIBRARY_STOCK → BOOK (One-to-One)
        // ========================================
        // Chaque livre a une entrée de stock unique

        modelBuilder.Entity<LibraryStockModel>()
            .HasOne<BookModel>()                // Un Stock a un Book
            .WithOne()                          // Un Book a un Stock
            .HasForeignKey<LibraryStockModel>(s => s.book_id)
            .OnDelete(DeleteBehavior.Cascade);  // Supprime le stock si le livre est supprimé

        // ========================================
        // RELATIONS BORROWED (CRITIQUES)
        // ========================================
        // Configuration explicite pour éviter les "shadow properties"

        modelBuilder.Entity<BorrowedModel>(entity =>
        {
            // ===== RELATION BORROWED → USER =====
            entity.HasOne(b => b.User)
                  .WithMany(u => u.BorrowedBooks)  // Collection inverse recommandée
                  .HasForeignKey(b => b.user_id)
                  .OnDelete(DeleteBehavior.Cascade);

            // ===== RELATION BORROWED → BOOK =====
            entity.HasOne(b => b.Book)
                  .WithMany()  // Pas de collection inverse nécessaire
                  .HasForeignKey(b => b.book_id)
                  .OnDelete(DeleteBehavior.Cascade);

            // ===== CONTRAINTE D'UNICITÉ =====
            // Un utilisateur ne peut emprunter qu'une seule fois le même livre
            // uniquement pour les emprunts non restitués (is_returned = 0)
            entity.HasIndex(b => new { b.user_id, b.book_id })
                  .IsUnique()
                  .HasFilter("[is_returned] = 0");  // Filtre SQL Server
        });

        base.OnModelCreating(modelBuilder);
    }
}


/// <summary>
/// ===== EXPLICATIONS COMPLÉMENTAIRES =====
/// 
/// 1. CLÉS PRIMAIRES :
///    - Identifient de manière unique chaque enregistrement
///    - Auto-incrémentées dans la base de données
/// 
/// 2. CLÉS ÉTRANGÈRES :
///    - Créent des liens entre les tables
///    - Garantissent l'intégrité référentielle
/// 
/// 3. COMPORTEMENTS DE SUPPRESSION :
///    - Cascade : Supprime les enregistrements liés
///    - Restrict : Empêche la suppression si des liens existent
/// 
/// 4. EAGER LOADING vs LAZY LOADING :
///    - Include() : Charge immédiatement les relations (Eager)
///    - Sans Include : Chargement à la demande (Lazy, nécessite config)
/// 
/// 5. SHADOW PROPERTIES :
///    - Propriétés créées automatiquement par EF Core
///    - Évitées ici par configuration explicite des relations
/// </summary>