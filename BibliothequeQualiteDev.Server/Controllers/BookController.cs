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
            return await _db.BOOK.ToListAsync();
        }

        [HttpGet("{id}")]
        public IActionResult GetBook(int id)
        {
            var book = _db.BOOK.Find(id);
            if (book == null) return NotFound();
            return Ok(book);
        }

        // Endpoint pour compter les exemplaires disponibles
        [HttpGet("{id}/available-count")]
        public async Task<IActionResult> GetAvailableCount(int id)
        {
            var stock = await _db.LIBRARY_STOCK.FirstOrDefaultAsync(s => s.book_id == id);
            if (stock == null) return Ok(0);
            return Ok(stock.AvailableCount);
        }

        [HttpPost]
        public async Task<IActionResult> AddBook([FromForm] BookUploadModel model)
        {
            var book = new BookModel
            {
                book_name = model.book_name ?? string.Empty,
                book_author = model.book_author ?? string.Empty,
                book_editor = model.book_editor ?? string.Empty,
                book_date = model.book_date
            };

            _db.BOOK.Add(book);
            await _db.SaveChangesAsync(); // ID généré ici

            // Gestion de l'image
            if (model.image != null && model.image.Length > 0)
            {
                var ext = Path.GetExtension(model.image.FileName);
                book.book_image_ext = ext;

                var dir = Path.Combine("wwwroot", "images", "books");
                Directory.CreateDirectory(dir);
                var path = Path.Combine(dir, $"{book.book_id}{ext}");

                using var stream = new FileStream(path, FileMode.Create);
                await model.image.CopyToAsync(stream);

                await _db.SaveChangesAsync();
            }

            // Ajout des exemplaires en stock
            int quantity = model.quantity > 0 ? model.quantity : 1;

            _db.LIBRARY_STOCK.Add(new LibraryStockModel
            {
                book_id = book.book_id,
                total_stock = quantity,
                borrowed_count = 0
            });

            await _db.SaveChangesAsync();
            return Ok(book);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(int id, [FromForm] BookUpdateDto dto, IFormFile? image)
        {
            var book = await _db.BOOK.FindAsync(id);
            if (book == null) return NotFound();

            book.book_name = dto.book_name ?? book.book_name;
            book.book_author = dto.book_author ?? book.book_author;
            book.book_editor = dto.book_editor ?? book.book_editor;
            book.book_date = dto.book_date;

            // Gestion image
            if (image != null && image.Length > 0)
            {
                var ext = Path.GetExtension(image.FileName).TrimStart('.');
                var imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "books");
                Directory.CreateDirectory(imagesPath);
                var filePath = Path.Combine(imagesPath, $"{book.book_id}.{ext}");

                using var stream = new FileStream(filePath, FileMode.Create);
                await image.CopyToAsync(stream);

                book.book_image_ext = "." + ext;
            }

            // Ajout d'exemplaires supplémentaires
            if (dto.quantity > 0)
            {
                var stock = await _db.LIBRARY_STOCK.FirstOrDefaultAsync(s => s.book_id == id);
                if (stock == null)
                {
                    stock = new LibraryStockModel { book_id = id, total_stock = dto.quantity };
                    _db.LIBRARY_STOCK.Add(stock);
                }
                else
                {
                    stock.total_stock += dto.quantity;
                }
            }

            await _db.SaveChangesAsync();
            return NoContent();

            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("{id}/borrow")]
        public async Task<IActionResult> BorrowBook(int id)
        {
            var userId = HttpContext.Session.GetInt32("user_id");
            if (!userId.HasValue) return Unauthorized("Connectez-vous pour emprunter.");

            // Vérifier si l'utilisateur a déjà emprunté ce livre (non rendu)
            var existingBorrow = await _db.BORROWED
                .FirstOrDefaultAsync(b => b.user_id == userId.Value &&
                                         b.book_id == id &&
                                         !b.is_returned);
            if (existingBorrow != null)
                return BadRequest("Vous avez déjà emprunté ce livre.");

            var stock = await _db.LIBRARY_STOCK.FirstOrDefaultAsync(s => s.book_id == id);
            if (stock == null || stock.AvailableCount <= 0)
                return BadRequest("Aucun exemplaire disponible.");

            var borrow = new BorrowedModel
            {
                user_id = userId.Value,
                book_id = id,
                date_start = DateTime.Today,
                date_end = DateTime.Today.AddDays(21),
                is_returned = false
            };

            _db.BORROWED.Add(borrow);
            stock.borrowed_count += 1; // Incrémenter le compteur

            await _db.SaveChangesAsync();

            return Ok(new { message = "Livre emprunté avec succès !" });
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteBook(int id)
        {
            var book = _db.BOOK.FirstOrDefault(b => b.book_id == id);
            if (book == null) return NotFound();

            _db.BOOK.Remove(book);
            _db.SaveChanges();
            return NoContent();
        }

        // DTOs
        public class BookUploadModel
        {
            public string? book_name { get; set; }
            public string? book_author { get; set; }
            public string? book_editor { get; set; }
            public DateTime book_date { get; set; }
            public IFormFile? image { get; set; }
            public int quantity { get; set; } = 1;
        }

        public class BookUpdateDto
        {
            public string? book_name { get; set; }
            public string? book_author { get; set; }
            public string? book_editor { get; set; }
            public DateTime book_date { get; set; }
            public int quantity { get; set; } = 0;
        }
    }
}