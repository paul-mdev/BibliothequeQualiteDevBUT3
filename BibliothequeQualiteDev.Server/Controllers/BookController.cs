using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Data;

namespace BibliothequeQualiteDev.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BookController : ControllerBase
    {

        private readonly AppDbContext _db;

        public BookController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IEnumerable<BookModel>> Get()
        {

            //Console.WriteLine(_db.BOOK.Count());

            // Charge toutes les lignes de la table WeatherRecords
            return await _db.BOOK.ToListAsync();
        }


        // GET /book/2
        [HttpGet("{id}")]
        public IActionResult GetBook(int id)
        {
            var book = _db.BOOK.Find(id);
            if (book == null) return NotFound();
            return Ok(book);
        }


        [HttpDelete("{id}")]
        public IActionResult DeleteBook(int id)
        {
            var book = _db.BOOK.FirstOrDefault(b => b.book_id == id);
            if (book == null) return NotFound();

            // Suppression directe, la cascade supprime le stock et les emprunts liés
            _db.BOOK.Remove(book);
            _db.SaveChanges();

            return NoContent();
        }




    }
}
