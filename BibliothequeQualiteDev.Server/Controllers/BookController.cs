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

        // GET /book → Liste tous les livres
        [HttpGet]
        public async Task<IEnumerable<BookModel>> Get()
        {
            return await _db.BOOK.ToListAsync();
        }

        // GET /book/{id} → Détail d'un livre
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBook(int id)
        {
            var book = await _db.BOOK.FindAsync(id);
            if (book == null) return NotFound();
            return Ok(book);
        }

        // GET /book/{id}/available-count → Nombre d'exemplaires disponibles
        [HttpGet("{id}/available-count")]
        public async Task<IActionResult> GetAvailableCount(int id)
        {
            var stock = await _db.LIBRARY_STOCK.FirstOrDefaultAsync(s => s.book_id == id);
            if (stock == null) return Ok(0);
            return Ok(stock.total_stock - stock.borrowed_count);
        }

        // POST /book → Ajout d'un nouveau livre
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
            await _db.SaveChangesAsync(); // Génère book.book_id

            // Gestion de l'image
            if (model.image != null && model.image.Length > 0)
            {
                var ext = Path.GetExtension(model.image.FileName);
                book.book_image_ext = ext;

                var dir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "books");
                Directory.CreateDirectory(dir);
                var path = Path.Combine(dir, $"{book.book_id}{ext}");

                using var stream = new FileStream(path, FileMode.Create);
                await model.image.CopyToAsync(stream);

                await _db.SaveChangesAsync(); // Met à jour book_image_ext
            }

            // Gestion du stock
            int quantity = model.quantity > 0 ? model.quantity : 1;
            var stock = new LibraryStockModel
            {
                book_id = book.book_id,
                total_stock = quantity,
                borrowed_count = 0
            };
            _db.LIBRARY_STOCK.Add(stock);
            await _db.SaveChangesAsync();

            return Ok(book);
        }

        // PUT /book/{id} → Modification d'un livre
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(int id, [FromForm] BookUpdateDto dto, IFormFile? image)
        {
            var book = await _db.BOOK.FindAsync(id);
            if (book == null) return NotFound();

            // Mise à jour des champs texte
            book.book_name = dto.book_name ?? book.book_name;
            book.book_author = dto.book_author ?? book.book_author;
            book.book_editor = dto.book_editor ?? book.book_editor;
            book.book_date = dto.book_date;

            // Gestion de l'image (remplacement si nouvelle image fournie)
            if (image != null && image.Length > 0)
            {
                var ext = Path.GetExtension(image.FileName);
                if (!string.IsNullOrEmpty(ext)) ext = ext.TrimStart('.');

                var imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "books");
                Directory.CreateDirectory(imagesPath);

                // Suppression de l'ancienne image si elle existe
                if (!string.IsNullOrEmpty(book.book_image_ext))
                {
                    var oldPath = Path.Combine(imagesPath, $"{book.book_id}{book.book_image_ext}");
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                var newPath = Path.Combine(imagesPath, $"{book.book_id}.{ext}");
                using var stream = new FileStream(newPath, FileMode.Create);
                await image.CopyToAsync(stream);

                book.book_image_ext = "." + ext;
            }

            // Ajout d'exemplaires supplémentaires
            if (dto.quantity > 0)
            {
                var stock = await _db.LIBRARY_STOCK.FirstOrDefaultAsync(s => s.book_id == id);
                if (stock == null)
                {
                    stock = new LibraryStockModel
                    {
                        book_id = id,
                        total_stock = dto.quantity,
                        borrowed_count = 0
                    };
                    _db.LIBRARY_STOCK.Add(stock);
                }
                else
                {
                    stock.total_stock += dto.quantity;
                }
            }

            await _db.SaveChangesAsync();
            return NoContent(); // 204 - Mise à jour réussie
        }

        // POST /book/{id}/borrow → Emprunter un livre
        [HttpPost("{id}/borrow")]
        public async Task<IActionResult> BorrowBook(int id)
        {
            var userId = HttpContext.Session.GetInt32("user_id");
            if (!userId.HasValue)
                return Unauthorized("Connectez-vous pour emprunter.");

            // Vérifier si déjà emprunté (non rendu)
            var existingBorrow = await _db.BORROWED
                .FirstOrDefaultAsync(b => b.user_id == userId.Value &&
                                         b.book_id == id &&
                                         !b.is_returned);

            if (existingBorrow != null)
                return BadRequest("Vous avez déjà emprunté ce livre.");

            var stock = await _db.LIBRARY_STOCK.FirstOrDefaultAsync(s => s.book_id == id);
            if (stock == null || stock.total_stock - stock.borrowed_count <= 0)
                return BadRequest("Aucun exemplaire disponible.");

            var borrow = new BorrowedModel
            {
                user_id = userId.Value,
                book_id = id,
                date_start = DateTime.Today,
                date_end = DateTime.Today.AddDays(60), // Durée d'emprunt : 60 jours
                is_returned = false
            };

            _db.BORROWED.Add(borrow);
            stock.borrowed_count += 1;

            await _db.SaveChangesAsync();

            return Ok(new { message = "Livre emprunté avec succès !" });
        }

        // DELETE /book/{id} → Suppression d'un livre (cascade via EF Core + DB)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await _db.BOOK.FirstOrDefaultAsync(b => b.book_id == id);
            if (book == null)
                return NotFound("Livre non trouvé.");

            _db.BOOK.Remove(book);
            await _db.SaveChangesAsync();

            return NoContent(); // 204 - Suppression réussie
        }

        // === DTOs ===
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