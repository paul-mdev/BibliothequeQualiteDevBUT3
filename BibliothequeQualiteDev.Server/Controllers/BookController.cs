using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BibliothequeQualiteDev.Server.Controllers
{
    /// <summary>
    /// ===== CONTRÔLEUR DE GESTION DES LIVRES =====
    /// Gère toutes les opérations CRUD sur les livres
    /// Route de base : /book
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class BookController : ControllerBase
    {
        private readonly AppDbContext _db;

        /// <summary>
        /// Constructeur avec injection de dépendances
        /// Le DbContext est automatiquement injecté par le conteneur DI
        /// </summary>
        public BookController(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// ===== GET /book =====
        /// Récupère la liste complète de tous les livres
        /// Utilisé pour l'affichage du catalogue
        /// </summary>
        /// <returns>Liste de tous les livres de la base de données</returns>
        [HttpGet]
        public async Task<IEnumerable<BookModel>> Get()
        {
            return await _db.BOOK.ToListAsync();
        }

        /// <summary>
        /// ===== GET /book/{id} =====
        /// Récupère les détails d'un livre spécifique
        /// Utilisé pour la page de détail du livre
        /// </summary>
        /// <param name="id">ID du livre recherché</param>
        /// <returns>Le livre trouvé ou NotFound (404)</returns>
        [HttpGet("{id}")]
        public IActionResult GetBook(int id)
        {
            var book = _db.BOOK.Find(id);
            if (book == null) return NotFound();
            return Ok(book);
        }

        /// <summary>
        /// ===== GET /book/{id}/available-count =====
        /// Compte le nombre d'exemplaires disponibles pour un livre
        /// Calcul : total_stock - borrowed_count
        /// Utilisé pour afficher la disponibilité en temps réel
        /// </summary>
        /// <param name="id">ID du livre</param>
        /// <returns>Nombre d'exemplaires disponibles</returns>
        [HttpGet("{id}/available-count")]
        public async Task<IActionResult> GetAvailableCount(int id)
        {
            var stock = await _db.LIBRARY_STOCK.FirstOrDefaultAsync(s => s.book_id == id);
            if (stock == null) return Ok(0);
            return Ok(stock.AvailableCount);
        }

        /// <summary>
        /// ===== POST /book =====
        /// Ajoute un nouveau livre dans la bibliothèque
        /// Processus en 3 étapes :
        /// 1. Création du livre
        /// 2. Upload de l'image (optionnel)
        /// 3. Création du stock initial
        /// </summary>
        /// <param name="model">Données du livre avec image et quantité</param>
        /// <returns>Le livre créé avec son ID généré</returns>
        [HttpPost]
        public async Task<IActionResult> AddBook([FromForm] BookUploadModel model)
        {
            // ===== ÉTAPE 1 : CRÉATION DU LIVRE =====
            var book = new BookModel
            {
                book_name = model.book_name ?? string.Empty,
                book_author = model.book_author ?? string.Empty,
                book_editor = model.book_editor ?? string.Empty,
                book_date = model.book_date
            };

            _db.BOOK.Add(book);
            await _db.SaveChangesAsync(); // L'ID est généré automatiquement ici

            // ===== ÉTAPE 2 : GESTION DE L'IMAGE =====
            if (model.image != null && model.image.Length > 0)
            {
                // Récupération de l'extension du fichier
                var ext = Path.GetExtension(model.image.FileName);
                book.book_image_ext = ext;

                // Création du dossier de destination
                var dir = Path.Combine("wwwroot", "images", "books");
                Directory.CreateDirectory(dir);

                // Nom du fichier : {book_id}{extension}
                var path = Path.Combine(dir, $"{book.book_id}{ext}");

                // Sauvegarde du fichier sur le disque
                using var stream = new FileStream(path, FileMode.Create);
                await model.image.CopyToAsync(stream);

                // Mise à jour du livre avec l'extension de l'image
                await _db.SaveChangesAsync();
            }

            // ===== ÉTAPE 3 : AJOUT DU STOCK INITIAL =====
            // Crée une entrée dans LIBRARY_STOCK avec la quantité spécifiée
            int quantity = model.quantity > 0 ? model.quantity : 1;

            _db.LIBRARY_STOCK.Add(new LibraryStockModel
            {
                book_id = book.book_id,
                total_stock = quantity,
                borrowed_count = 0  // Aucun emprunt au départ
            });

            await _db.SaveChangesAsync();
            return Ok(book);
        }

        /// <summary>
        /// ===== PUT /book/{id} =====
        /// Modifie un livre existant
        /// Permet de :
        /// - Mettre à jour les informations du livre
        /// - Changer l'image de couverture
        /// - Ajouter des exemplaires au stock
        /// </summary>
        /// <param name="id">ID du livre à modifier</param>
        /// <param name="dto">Nouvelles données du livre</param>
        /// <param name="image">Nouvelle image (optionnelle)</param>
        /// <returns>NoContent (204) si succès</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(int id, [FromForm] BookUpdateDto dto, IFormFile? image)
        {
            var book = await _db.BOOK.FindAsync(id);
            if (book == null) return NotFound();

            // ===== MISE À JOUR DES INFORMATIONS =====
            // Utilise l'opérateur ?? pour conserver l'ancienne valeur si null
            book.book_name = dto.book_name ?? book.book_name;
            book.book_author = dto.book_author ?? book.book_author;
            book.book_editor = dto.book_editor ?? book.book_editor;
            book.book_date = dto.book_date;

            // ===== MISE À JOUR DE L'IMAGE =====
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

            // ===== AJOUT D'EXEMPLAIRES SUPPLÉMENTAIRES =====
            // Si quantity > 0, ajoute des exemplaires au stock existant
            if (dto.quantity > 0)
            {
                var stock = await _db.LIBRARY_STOCK.FirstOrDefaultAsync(s => s.book_id == id);
                if (stock == null)
                {
                    // Création du stock s'il n'existe pas
                    stock = new LibraryStockModel { book_id = id, total_stock = dto.quantity };
                    _db.LIBRARY_STOCK.Add(stock);
                }
                else
                {
                    // Incrémente le stock existant
                    stock.total_stock += dto.quantity;
                }
            }

            await _db.SaveChangesAsync();
            return NoContent();

            // NOTE : Code dupliqué ci-dessous (probablement une erreur)
            await _db.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// ===== POST /book/{id}/borrow =====
        /// Emprunte un livre pour l'utilisateur connecté
        /// Vérifications effectuées :
        /// 1. Utilisateur authentifié
        /// 2. Pas d'emprunt en cours du même livre
        /// 3. Au moins un exemplaire disponible
        /// </summary>
        /// <param name="id">ID du livre à emprunter</param>
        /// <returns>Message de confirmation ou erreur</returns>
        [HttpPost("{id}/borrow")]
        public async Task<IActionResult> BorrowBook(int id)
        {
            // ===== VÉRIFICATION AUTHENTIFICATION =====
            var userId = HttpContext.Session.GetInt32("user_id");
            if (!userId.HasValue) return Unauthorized("Connectez-vous pour emprunter.");

            // ===== VÉRIFICATION EMPRUNT EXISTANT =====
            // Un utilisateur ne peut pas emprunter deux fois le même livre
            var existingBorrow = await _db.BORROWED
                .FirstOrDefaultAsync(b => b.user_id == userId.Value &&
                                         b.book_id == id &&
                                         !b.is_returned);
            if (existingBorrow != null)
                return BadRequest("Vous avez déjà emprunté ce livre.");

            // ===== VÉRIFICATION DISPONIBILITÉ =====
            var stock = await _db.LIBRARY_STOCK.FirstOrDefaultAsync(s => s.book_id == id);
            if (stock == null || stock.AvailableCount <= 0)
                return BadRequest("Aucun exemplaire disponible.");

            // ===== CRÉATION DE L'EMPRUNT =====
            var borrow = new BorrowedModel
            {
                user_id = userId.Value,
                book_id = id,
                date_start = DateTime.Today,
                date_end = DateTime.Today.AddDays(60),  // Durée d'emprunt : 60 jours
                is_returned = false
            };

            _db.BORROWED.Add(borrow);
            stock.borrowed_count += 1; // Incrémente le compteur d'emprunts

            await _db.SaveChangesAsync();

            return Ok(new { message = "Livre emprunté avec succès !" });
        }

        /// <summary>
        /// ===== DELETE /book/{id} =====
        /// Supprime un livre de la bibliothèque
        /// ATTENTION : Supprime également le stock et les emprunts associés
        /// (selon les règles de cascade définies dans DbContext)
        /// </summary>
        /// <param name="id">ID du livre à supprimer</param>
        /// <returns>NoContent (204) si succès</returns>
        [HttpDelete("{id}")]
        public IActionResult DeleteBook(int id)
        {
            var book = _db.BOOK.FirstOrDefault(b => b.book_id == id);
            if (book == null) return NotFound();

            _db.BOOK.Remove(book);
            _db.SaveChanges();
            return NoContent();
        }

        // ===== DTOs (DATA TRANSFER OBJECTS) =====

        /// <summary>
        /// DTO pour l'ajout d'un livre
        /// Inclut les informations du livre, l'image et la quantité initiale
        /// [FromForm] permet de recevoir les données en multipart/form-data
        /// </summary>
        public class BookUploadModel
        {
            public string? book_name { get; set; }
            public string? book_author { get; set; }
            public string? book_editor { get; set; }
            public DateTime book_date { get; set; }
            public IFormFile? image { get; set; }  // Fichier image uploadé
            public int quantity { get; set; } = 1;  // Quantité par défaut
        }

        /// <summary>
        /// DTO pour la modification d'un livre
        /// Quantité = 0 par défaut (pas d'ajout d'exemplaires)
        /// Si > 0, ajoute des exemplaires au stock existant
        /// </summary>
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