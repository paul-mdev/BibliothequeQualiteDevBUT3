using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("[controller]")]
public class StatisticsController : ControllerBase
{
    private readonly AppDbContext _db;

    public StatisticsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<StatisticsDto>> GetStatistics()
    {
        var dto = new StatisticsDto();

        // Nombre total de livres (dans la table BOOK)
        dto.TotalBooks = await _db.BOOK.CountAsync();

        // Nombre total d'utilisateurs
        dto.TotalUsers = await _db.USERS.CountAsync();

        // Nombre total d'emprunts (historique)
        dto.TotalBorrowings = await _db.BORROWED.CountAsync();

        // Nombre d'emprunts en cours
        dto.CurrentBorrowings = await _db.BORROWED.CountAsync(b => !b.is_returned);

        // Nombre total de retards enregistrés
        dto.TotalDelays = await _db.DELAY.CountAsync();

        // Taux de retard
        dto.DelayRate = dto.TotalBorrowings > 0
            ? Math.Round((double)dto.TotalDelays / dto.TotalBorrowings * 100, 2)
            : 0;

        // Livres les plus populaires (top 10 par nombre total d'emprunts)
        dto.PopularBooks = await _db.BORROWED
            .GroupBy(b => b.book_id)
            .Select(g => new { BookId = g.Key, BorrowCount = g.Count() })
            .OrderByDescending(g => g.BorrowCount)
            .Take(10)
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

        // Optionnel : répartition du stock (disponible / emprunté / total)
        // Tu peux ajouter cela si tu veux l'afficher dans les stats
        // Optionnel : répartition du stock (disponible / emprunté / total)
        dto.StockByState = new List<StockByState>
        {
            new StockByState { StateId = 1, Count = await _db.LIBRARY_STOCK.SumAsync(s => s.total_stock) },
            new StockByState { StateId = 2, Count = await _db.LIBRARY_STOCK.SumAsync(s => s.borrowed_count) },
            new StockByState
            {
                StateId = 3,
                Count = await _db.LIBRARY_STOCK.SumAsync(s => s.total_stock - s.borrowed_count)
            }
        };
        return Ok(dto);
    }
}

// DTOs (inchangés sauf ajout de noms clairs pour StockByState si tu l'utilises)
public class StatisticsDto
{
    public int TotalBooks { get; set; }
    public int TotalUsers { get; set; }
    public int TotalBorrowings { get; set; }
    public int CurrentBorrowings { get; set; }
    public int TotalDelays { get; set; }
    public double DelayRate { get; set; } // en pourcentage
    public List<BookPopularity> PopularBooks { get; set; } = new List<BookPopularity>();
    public List<StockByState> StockByState { get; set; } = new List<StockByState>();
}

public class BookPopularity
{
    public int BookId { get; set; }
    public string BookName { get; set; } = string.Empty;
    public int BorrowCount { get; set; }
}

public class StockByState
{
    public int StateId { get; set; } // 1 = Total exemplaires, 2 = Empruntés, 3 = Disponibles
    public int Count { get; set; }
}