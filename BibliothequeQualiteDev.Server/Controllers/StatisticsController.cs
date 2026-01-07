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

        dto.TotalBooks = await _db.BOOK.CountAsync();
        dto.TotalUsers = await _db.USERS.CountAsync();
        dto.TotalBorrowings = await _db.BORROWED.CountAsync();
        dto.CurrentBorrowings = await _db.BORROWED.CountAsync(b => !b.is_returned);
        dto.TotalDelays = await _db.DELAY.CountAsync();

        dto.DelayRate = dto.TotalBorrowings > 0
            ? Math.Round((double)dto.TotalDelays / dto.TotalBorrowings * 100, 2)
            : 0;

        // Top 15 livres les plus empruntés (historique complet)
        dto.PopularBooks = await _db.BORROWED
            .GroupBy(b => b.book_id)
            .Select(g => new { BookId = g.Key, BorrowCount = g.Count() })
            .OrderByDescending(g => g.BorrowCount)
            .Take(15) // ← 15 au lieu de 10
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

        // Statistiques de stock global
        var totalStock = await _db.LIBRARY_STOCK.SumAsync(s => s.total_stock);
        var borrowedCount = await _db.LIBRARY_STOCK.SumAsync(s => s.borrowed_count);

        dto.StockByState = new List<StockByState>
        {
            new StockByState { StateId = 1, Count = totalStock },         // Total exemplaires
            new StockByState { StateId = 2, Count = borrowedCount },      // Empruntés actuellement
            new StockByState { StateId = 3, Count = totalStock - borrowedCount } // Disponibles
        };

        return Ok(dto);
    }
}

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

public class BookPopularity
{
    public int BookId { get; set; }
    public string BookName { get; set; } = string.Empty;
    public int BorrowCount { get; set; }
}

public class StockByState
{
    public int StateId { get; set; } // 1=Total, 2=Empruntés, 3=Disponibles
    public int Count { get; set; }
}