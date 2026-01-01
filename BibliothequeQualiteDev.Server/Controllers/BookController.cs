using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            Console.WriteLine("zaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
            var book = _db.BOOK.Find(id);
            if (book == null) return NotFound();
            return Ok(book);
        }


    }
}
