using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BibliothequeQualiteDev.Server.Models;

[ApiController]
[Route("[controller]")]
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
        var userIdStr = HttpContext.Session.GetString("user_id");
        if (string.IsNullOrEmpty(userIdStr))
            return Unauthorized();

        if (!int.TryParse(userIdStr, out int userId))
            return Unauthorized();

        // On récupère les emprunts + le stock + le livre associé
        var borrowed = await (
            from b in _db.BORROWED
           
            join bk in _db.BOOK on b.book_id equals bk.book_id
            where b.user_id == userId
            select new
            {
                b.id_borrow,
                b.date_start,
                b.date_end,
                b.is_returned,
                BookId = bk.book_id,
                BookName = bk.book_name,
                BookAuthor = bk.book_author
            }
        ).ToListAsync();

        return Ok(borrowed);
    }
}
