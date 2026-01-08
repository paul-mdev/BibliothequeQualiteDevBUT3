using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BibliothequeQualiteDev.Server.Controllers
{
    /// <summary>
    /// ===== CONTRÔLEUR DE GESTION DES EMPRUNTS =====
    /// Gère les opérations liées aux emprunts de livres
    /// Route de base : /borrow
    /// Principaux endpoints :
    /// - GET /borrow/current : Liste des emprunts en cours
    /// - POST /borrow/{id}/return : Marquer un emprunt comme restitué
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class BorrowController : ControllerBase
    {
        private readonly AppDbContext _db;

        /// <summary>
        /// Constructeur avec injection du DbContext
        /// </summary>
        public BorrowController(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// ===== GET /borrow/current =====
        /// Récupère la liste de tous les emprunts en cours (non restitués)
        /// Utilisé pour la page de gestion des emprunts
        /// 
        /// Inclut :
        /// - Informations du livre (via Include)
        /// - Informations de l'emprunteur (via Include)
        /// - Dates d'emprunt et de retour prévue
        /// 
        /// Tri : Par date de retour croissante (les plus urgents en premier)
        /// </summary>
        /// <returns>Liste des emprunts en cours avec détails</returns>
        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentBorrows()
        {
            var currentBorrows = await _db.BORROWED
                // ===== FILTRE : Uniquement les emprunts non restitués =====
                .Where(b => !b.is_returned)

                // ===== EAGER LOADING : Chargement des relations =====
                // Include évite les requêtes supplémentaires (N+1 problem)
                .Include(b => b.Book)   // Charge les infos du livre
                .Include(b => b.User)   // Charge les infos de l'utilisateur

                // ===== PROJECTION : Sélection des champs nécessaires =====
                // Évite de retourner des objets complets (meilleures perfs)
                .Select(b => new
                {
                    b.id_borrow,
                    book_name = b.Book.book_name,
                    user_name = b.User.user_name,
                    user_mail = b.User.user_mail,
                    b.date_start,
                    b.date_end
                })

                // ===== TRI : Par date de retour =====
                // Les emprunts qui doivent être rendus en premier apparaissent en haut
                .OrderBy(b => b.date_end)
                .ToListAsync();

            return Ok(currentBorrows);
        }

        /// <summary>
        /// ===== POST /borrow/{borrowId}/return =====
        /// Marque un emprunt comme restitué
        /// 
        /// Opérations effectuées :
        /// 1. Vérification de l'existence de l'emprunt
        /// 2. Vérification qu'il n'est pas déjà restitué
        /// 3. Mise à jour du statut de l'emprunt
        /// 4. Décrémentation du compteur d'emprunts dans le stock
        /// 
        /// Note : Cette opération libère un exemplaire du livre
        /// </summary>
        /// <param name="borrowId">ID de l'emprunt à restituer</param>
        /// <returns>Message de confirmation ou erreur</returns>
        [HttpPost("{borrowId}/return")]
        public async Task<IActionResult> ReturnBook(int borrowId)
        {
            // ===== RÉCUPÉRATION DE L'EMPRUNT =====
            // Include(b => b.Book) charge les infos du livre pour accéder à book_id
            var borrow = await _db.BORROWED
                .Include(b => b.Book)
                .FirstOrDefaultAsync(b => b.id_borrow == borrowId);

            // ===== VÉRIFICATION EXISTENCE =====
            if (borrow == null)
                return NotFound("Emprunt non trouvé.");

            // ===== VÉRIFICATION STATUT =====
            // Empêche de restituer un livre déjà rendu
            if (borrow.is_returned)
                return BadRequest("Ce livre a déjà été restitué.");

            // ===== MISE À JOUR DE L'EMPRUNT =====
            borrow.is_returned = true;

            // ===== MISE À JOUR DU STOCK =====
            // Décrémente le compteur d'emprunts pour rendre l'exemplaire disponible
            var stock = await _db.LIBRARY_STOCK
                .FirstOrDefaultAsync(s => s.book_id == borrow.book_id);

            if (stock != null)
            {
                // Math.Max garantit que le compteur ne devient jamais négatif
                // (protection contre les incohérences de données)
                stock.borrowed_count = Math.Max(0, stock.borrowed_count - 1);
            }

            // ===== SAUVEGARDE DES CHANGEMENTS =====
            await _db.SaveChangesAsync();

            return Ok(new { message = "Livre restitué avec succès." });
        }
    }
}