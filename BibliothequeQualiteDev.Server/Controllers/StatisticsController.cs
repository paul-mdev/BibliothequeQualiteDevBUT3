using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[ApiController]
[Route("[controller]")]
public class StatisticsController : ControllerBase
{
    private readonly AppDbContext _db;

    public StatisticsController(AppDbContext db)
    {
        _db = db;
    }

    // GET /statistics
    [HttpGet]
    public async Task<ActionResult<StatisticsDto>> GetStatistics()
    {
        var dto = new StatisticsDto();

        // Nombre total de livres
        dto.TotalBooks = await _db.BOOK.CountAsync();

        // Nombre total d'utilisateurs
        dto.TotalUsers = await _db.USERS.CountAsync();

        // Nombre total d'emprunts
        dto.TotalBorrowings = await _db.BORROWED.CountAsync();

        // Nombre d'emprunts en cours (non retournés)
        dto.CurrentBorrowings = await _db.BORROWED.CountAsync(b => !b.is_returned);

        // Nombre total de retards
        dto.TotalDelays = await _db.DELAY.CountAsync();

        // Taux de retard (pourcentage)
        dto.DelayRate = dto.TotalBorrowings > 0 ? (double)dto.TotalDelays / dto.TotalBorrowings * 100 : 0;

        // Livres les plus populaires (top 10 par nombre d'emprunts)
        dto.PopularBooks = await _db.BORROWED
            .GroupBy(b => b.stock_id)
            .Select(g => new { StockId = g.Key, BorrowCount = g.Count() })
            .Join(_db.LIBRARY_STOCK, g => g.StockId, s => s.stock_id, (g, s) => new { s.book_id, g.BorrowCount })
            .GroupBy(x => x.book_id)
            .Select(g => new { BookId = g.Key, TotalBorrowCount = g.Sum(x => x.BorrowCount) })
            .OrderByDescending(g => g.TotalBorrowCount)
            .Take(10)
            .Join(_db.BOOK, g => g.BookId, b => b.book_id, (g, b) => new BookPopularity
            {
                BookId = g.BookId,
                BookName = b.book_name,
                BorrowCount = g.TotalBorrowCount
            })
            .ToListAsync();

        // Ajoutez d'autres stats si needed, ex: nombre de livres en stock par état
        dto.StockByState = await _db.LIBRARY_STOCK
            .GroupBy(s => s.state_id)
            .Select(g => new StockByState
            {
                StateId = g.Key,
                Count = g.Count()
            })
            .ToListAsync();

        return Ok(dto);
    }
}

// DTO pour renvoyer les stats
public class StatisticsDto
{
    public int TotalBooks { get; set; }
    public int TotalUsers { get; set; }
    public int TotalBorrowings { get; set; }
    public int CurrentBorrowings { get; set; }
    public int TotalDelays { get; set; }
    public double DelayRate { get; set; }
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
    public int StateId { get; set; }
    public int Count { get; set; }
}