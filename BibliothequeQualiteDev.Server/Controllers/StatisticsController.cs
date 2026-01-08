using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// ===== CONTRÔLEUR DES STATISTIQUES =====
/// Fournit toutes les statistiques de la bibliothèque
/// Route de base : /statistics
/// 
/// Statistiques fournies :
/// - Compteurs globaux (livres, utilisateurs, emprunts)
/// - Taux de retard
/// - Top 15 des livres les plus empruntés
/// - Répartition du stock (total, empruntés, disponibles)
/// </summary>
[ApiController]
[Route("[controller]")]
public class StatisticsController : ControllerBase
{
    private readonly AppDbContext _db;

    public StatisticsController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// ===== GET /statistics =====
    /// Calcule et retourne toutes les statistiques de la bibliothèque
    /// Utilisé pour alimenter le dashboard/tableau de bord
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<StatisticsDto>> GetStatistics()
    {
        var dto = new StatisticsDto();

        // ===== COMPTEURS GLOBAUX =====
        // Nombre total de livres dans le catalogue
        dto.TotalBooks = await _db.BOOK.CountAsync();

        // Nombre total d'utilisateurs inscrits
        dto.TotalUsers = await _db.USERS.CountAsync();

        // Nombre total d'emprunts (historique complet)
        dto.TotalBorrowings = await _db.BORROWED.CountAsync();

        // Nombre d'emprunts actuellement en cours (non restitués)
        dto.CurrentBorrowings = await _db.BORROWED.CountAsync(b => !b.is_returned);

        // Nombre total de retards enregistrés
        dto.TotalDelays = await _db.DELAY.CountAsync();

        // ===== CALCUL DU TAUX DE RETARD =====
        // Pourcentage d'emprunts ayant généré un retard
        // Arrondi à 2 décimales
        dto.DelayRate = dto.TotalBorrowings > 0
            ? Math.Round((double)dto.TotalDelays / dto.TotalBorrowings * 100, 2)
            : 0;

        // ===== TOP 15 DES LIVRES LES PLUS EMPRUNTÉS =====
        // Requête en plusieurs étapes :
        // 1. GroupBy : Regroupe les emprunts par book_id
        // 2. Select : Compte le nombre d'emprunts par livre
        // 3. OrderByDescending : Trie par nombre d'emprunts décroissant
        // 4. Take(15) : Prend les 15 premiers
        // 5. Join : Récupère les informations du livre
        dto.PopularBooks = await _db.BORROWED
            .GroupBy(b => b.book_id)
            .Select(g => new { BookId = g.Key, BorrowCount = g.Count() })
            .OrderByDescending(g => g.BorrowCount)
            .Take(15)  // Top 15
            .Join(_db.BOOK,
                  g => g.BookId,
                  b => b.book_id,
                  (g, b) => new BookPopularity
                  {
                      BookId = b.book_id,
                      BookName = b.book_name,
                      BorrowCount = g.BorrowCount
                  })
            .ToListAsync();

        // ===== STATISTIQUES DE STOCK GLOBAL =====
        // Somme de tous les exemplaires (tous livres confondus)
        var totalStock = await _db.LIBRARY_STOCK.SumAsync(s => s.total_stock);
        // Somme des exemplaires actuellement empruntés
        var borrowedCount = await _db.LIBRARY_STOCK.SumAsync(s => s.borrowed_count);

        // ===== RÉPARTITION DU STOCK =====
        // StateId 1 : Total des exemplaires
        // StateId 2 : Exemplaires empruntés actuellement
        // StateId 3 : Exemplaires disponibles
        dto.StockByState = new List<StockByState>
        {
            new StockByState { StateId = 1, Count = totalStock },
            new StockByState { StateId = 2, Count = borrowedCount },
            new StockByState { StateId = 3, Count = totalStock - borrowedCount }
        };

        return Ok(dto);
    }
}

/// <summary>
/// DTO principal contenant toutes les statistiques
/// </summary>
public class StatisticsDto
{
    public int TotalBooks { get; set; }
    public int TotalUsers { get; set; }
    public int TotalBorrowings { get; set; }
    public int CurrentBorrowings { get; set; }
    public int TotalDelays { get; set; }
    public double DelayRate { get; set; }
    public List<BookPopularity> PopularBooks { get; set; } = new();
    public List<StockByState> StockByState { get; set; } = new();
}

/// <summary>
/// DTO pour la popularité d'un livre
/// </summary>
public class BookPopularity
{
    public int BookId { get; set; }
    public string BookName { get; set; } = string.Empty;
    public int BorrowCount { get; set; }  // Nombre de fois emprunté
}

/// <summary>
/// DTO pour l'état du stock
/// </summary>
public class StockByState
{
    public int StateId { get; set; }  // 1=Total, 2=Empruntés, 3=Disponibles
    public int Count { get; set; }
}