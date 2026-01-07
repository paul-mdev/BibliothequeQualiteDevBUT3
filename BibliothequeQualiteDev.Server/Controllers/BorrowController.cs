using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BibliothequeQualiteDev.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BorrowController : ControllerBase
    {
        private readonly AppDbContext _db;

        public BorrowController(AppDbContext db)
        {
            _db = db;
        }

        // GET /borrow/current → liste des emprunts en cours
        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentBorrows()
        {
            var currentBorrows = await _db.BORROWED
                .Where(b => !b.is_returned)
                .Include(b => b.Book)
                .Include(b => b.User)
                .Select(b => new
                {
                    b.id_borrow,
                    book_name = b.Book.book_name,
                    user_name = b.User.user_name,
                    user_mail = b.User.user_mail,
                    b.date_start,
                    b.date_end
                })
                .OrderBy(b => b.date_end)
                .ToListAsync();

            return Ok(currentBorrows);
        }

        // POST /borrow/{borrowId}/return → marquer comme restitué
        [HttpPost("{borrowId}/return")]
        public async Task<IActionResult> ReturnBook(int borrowId)
        {
            var borrow = await _db.BORROWED
                .Include(b => b.Book)
                .FirstOrDefaultAsync(b => b.id_borrow == borrowId);

            if (borrow == null)
                return NotFound("Emprunt non trouvé.");

            if (borrow.is_returned)
                return BadRequest("Ce livre a déjà été restitué.");

            borrow.is_returned = true;

            // Mise à jour du stock
            var stock = await _db.LIBRARY_STOCK
                .FirstOrDefaultAsync(s => s.book_id == borrow.book_id);

            if (stock != null)
            {
                stock.borrowed_count = Math.Max(0, stock.borrowed_count - 1);
            }

            await _db.SaveChangesAsync();

            return Ok(new { message = "Livre restitué avec succès." });
        }
    }
}