using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("[controller]")] // Route: /UsersBorrowed
public class UsersBorrowedController : ControllerBase
{
    private readonly AppDbContext _db;

    public UsersBorrowedController(AppDbContext db)
    {
        _db = db;
    }

    // GET /UsersBorrowed/me
    [HttpGet("me")]
    public async Task<IActionResult> GetMyBorrowed()
    {
        // Récupérer l'utilisateur connecté depuis la session
        var userId = HttpContext.Session.GetInt32("user_id");

        Console.WriteLine($"[UsersBorrowed/me] Session user_id: {userId}");

        if (userId == null)
        {
            Console.WriteLine("[UsersBorrowed/me] ❌ Non autorisé - pas de session");
            return Unauthorized();
        }

        try
        {
            // Récupérer les emprunts de l'utilisateur
            var borrowed = await _db.BORROWED
                .Where(b => b.user_id == userId.Value)
                .Join(_db.BOOK,
                    borrow => borrow.book_id,
                    book => book.book_id,
                    (borrow, book) => new
                    {
                        borrow.id_borrow,
                        borrow.date_start,
                        borrow.date_end,
                        borrow.is_returned,
                        BookId = book.book_id,
                        BookName = book.book_name,
                        BookAuthor = book.book_author
                    })
                .ToListAsync();

            Console.WriteLine($"[UsersBorrowed/me] ✅ {borrowed.Count} emprunts trouvés");

            return Ok(borrowed);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[UsersBorrowed/me] ❌ Erreur: {ex.Message}");
            return StatusCode(500, "Erreur serveur");
        }
    }

    // GET /UsersBorrowed/all (optionnel - pour les admins)
    [HttpGet("all")]
    public async Task<IActionResult> GetAllBorrowed()
    {
        var userId = HttpContext.Session.GetInt32("user_id");
        if (userId == null) return Unauthorized();

        try
        {
            var borrowed = await _db.BORROWED
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
            Console.WriteLine($"[UsersBorrowed/all] ❌ Erreur: {ex.Message}");
            return StatusCode(500, "Erreur serveur");
        }
    }
}