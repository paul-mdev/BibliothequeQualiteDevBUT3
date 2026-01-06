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

        [HttpPost]
        public async Task<IActionResult> AddBook([FromForm] BookUploadModel model)
        {
            Console.WriteLine("AddBook");
            var book = new BookModel
            {
                book_name = model.book_name,
                book_author = model.book_author,
                book_editor = model.book_editor,
                book_date = model.book_date
            };

            _db.BOOK.Add(book);
            Console.WriteLine("ajoute en base");
            await _db.SaveChangesAsync(); // ⬅️ ID généré ICI

            //if (model.image != null && model.image.Length > 0)
            //{
            //    var ext = Path.GetExtension(model.image.FileName);
            //    book.book_image_ext = ext;

            //    var dir = Path.Combine("wwwroot/images/books");
            //    Directory.CreateDirectory(dir);
            //    Console.WriteLine("créé le chemin");

            //    var path = Path.Combine(dir, $"{book.book_id}{ext}");
            //    Console.WriteLine(path);

            //    using var stream = new FileStream(path, FileMode.Create);
            //    await model.image.CopyToAsync(stream);

            //    await _db.SaveChangesAsync();
            //    Console.WriteLine("changement effectué");

            //}

            return Ok(book);
        }



        // Classe pour recevoir le formulaire
        public class BookUploadModel
        {
            public string book_name { get; set; }
            public string book_author { get; set; }
            public string book_editor { get; set; }
            public DateTime book_date { get; set; }

            public IFormFile? image { get; set; }
        }

    }
}
