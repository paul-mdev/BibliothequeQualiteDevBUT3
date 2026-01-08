using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// ===== CONTRÔLEUR DES EMPRUNTS UTILISATEUR =====
/// Fournit les emprunts d'un utilisateur spécifique
/// Route de base : /UsersBorrowed
/// 
/// Endpoints :
/// - GET /UsersBorrowed/me : Emprunts de l'utilisateur connecté
/// - GET /UsersBorrowed/all : Tous les emprunts (pour admins)
/// </summary>
[ApiController]
[Route("[controller]")]
public class UsersBorrowedController : ControllerBase
{
    private readonly AppDbContext _db;

    public UsersBorrowedController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// ===== GET /UsersBorrowed/me =====
    /// Récupère tous les emprunts de l'utilisateur connecté
    /// Inclut les emprunts en cours ET l'historique (restitués)
    /// Utilisé pour la page "Mon compte" / "Mes emprunts"
    /// 
    /// Sécurité : Nécessite d'être authentifié (session)
    /// Chaque utilisateur ne voit que ses propres emprunts
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMyBorrowed()
    {
        // ===== RÉCUPÉRATION DE L'ID UTILISATEUR =====
        // Depuis la session côté serveur
        var userId = HttpContext.Session.GetInt32("user_id");


        // ===== VÉRIFICATION AUTHENTIFICATION =====
        if (userId == null)
        {
            return Unauthorized();
        }

        try
        {
            // ===== RÉCUPÉRATION DES EMPRUNTS =====
            // Requête avec JOIN pour récupérer les infos du livre
            var borrowed = await _db.BORROWED
                // Filtre : uniquement les emprunts de cet utilisateur
                .Where(b => b.user_id == userId.Value)

                // JOIN avec la table BOOK pour récupérer les détails
                .Join(_db.BOOK,
                    borrow => borrow.book_id,
                    book => book.book_id,
                    (borrow, book) => new
                    {
                        borrow.id_borrow,
                        borrow.date_start,
                        borrow.date_end,
                        borrow.is_returned,  // Important : statut rendu/non rendu
                        BookId = book.book_id,
                        BookName = book.book_name,
                        BookAuthor = book.book_author
                    })
                .ToListAsync();


            return Ok(borrowed);
        }
        catch (Exception ex)
        {
            // ===== GESTION DES ERREURS =====
            return StatusCode(500, "Erreur serveur");
        }
    }

    /// <summary>
    /// ===== GET /UsersBorrowed/all =====
    /// Récupère TOUS les emprunts de TOUS les utilisateurs
    /// Endpoint optionnel pour les administrateurs
    /// 
    /// Note : Dans la version actuelle, nécessite juste d'être connecté
    /// TODO : Ajouter une vérification de rôle administrateur
    /// </summary>
    [HttpGet("all")]
    public async Task<IActionResult> GetAllBorrowed()
    {
        // ===== VÉRIFICATION BASIQUE =====
        var userId = HttpContext.Session.GetInt32("user_id");
        if (userId == null) return Unauthorized();

        // TODO : Vérifier que l'utilisateur a le rôle "admin"
        // if (!IsAdmin(userId)) return Forbid();

        try
        {
            // ===== REQUÊTE COMPLEXE AVEC DOUBLE JOIN =====
            // 1. BORROWED → BOOK
            // 2. Résultat → USERS
            var borrowed = await _db.BORROWED
                // Premier JOIN : Emprunts + Livres
                .Join(_db.BOOK,
                    borrow => borrow.book_id,
                    book => book.book_id,
                    (borrow, book) => new
                    {
                        borrow.id_borrow,
                        borrow.user_id,
                        borrow.date_start,
                        borrow.date_end,
                        borrow.is_returned,
                        BookName = book.book_name,
                        BookAuthor = book.book_author
                    })
                // Deuxième JOIN : Résultat + Utilisateurs
                .Join(_db.USERS,
                    b => b.user_id,
                    u => u.user_id,
                    (b, u) => new
                    {
                        b.id_borrow,
                        b.user_id,
                        user_name = u.user_name,
                        user_mail = u.user_mail,
                        b.date_start,
                        b.date_end,
                        b.is_returned,
                        b.BookName,
                        b.BookAuthor
                    })
                .ToListAsync();

            return Ok(borrowed);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Erreur serveur");
        }
    }
}