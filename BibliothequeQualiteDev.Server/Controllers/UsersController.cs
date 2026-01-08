using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BibliothequeQualiteDev.Server.Models;
using BCrypt.Net;

namespace BibliothequeQualiteDev.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _db;
        public UsersController(AppDbContext db)
        {
            _db = db;
        }

        public class UsersCreateDTO
        {
            public string user_name { get; set; } = string.Empty;
            public string user_mail { get; set; } = string.Empty;
            public string user_pswd { get; set; } = string.Empty;
            public int role_id { get; set; }
        }

        // GET /users → liste tous les utilisateurs avec leur rôle
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _db.USERS
                .Include(u => u.role)
                .Select(u => new {
                    u.user_id,
                    u.user_name,
                    u.user_mail,
                    u.role_id,
                    role_name = u.role != null ? u.role.role_name : "N/A"
                })
                .ToListAsync();

            Console.WriteLine($"[Users] Retour de {users.Count} utilisateurs");
            return Ok(users);
        }

        // GET /users/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _db.USERS
                .Include(u => u.role)
                .Where(u => u.user_id == id)
                .Select(u => new {
                    u.user_id,
                    u.user_name,
                    u.user_mail,
                    u.role_id,
                    role_name = u.role != null ? u.role.role_name : "N/A"
                })
                .FirstOrDefaultAsync();

            if (user == null) return NotFound();
            return Ok(user);
        }

        // POST /users → création
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] UsersCreateDTO dto)
        {
            if (string.IsNullOrEmpty(dto.user_pswd))
                return BadRequest("Mot de passe requis");

            if (_db.USERS.Any(u => u.user_mail == dto.user_mail))
                return BadRequest("Email déjà utilisé");

            var user = new UsersModel
            {
                user_name = dto.user_name,
                user_mail = dto.user_mail,
                user_pswd = BCrypt.Net.BCrypt.HashPassword(dto.user_pswd),
                role_id = dto.role_id
            };

            _db.USERS.Add(user);
            await _db.SaveChangesAsync();

            Console.WriteLine($"[Users] Utilisateur créé: {user.user_id}");

            return Ok(new
            {
                user.user_id,
                user.user_name,
                user.user_mail,
                user.role_id
            });
        }

        // PUT /users/{id} → modification
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UsersCreateDTO dto)
        {
            var user = await _db.USERS.FindAsync(id);
            if (user == null) return NotFound();

            user.user_name = dto.user_name;
            user.user_mail = dto.user_mail;
            user.role_id = dto.role_id;

            // Ne hasher que si un nouveau mot de passe est fourni
            if (!string.IsNullOrEmpty(dto.user_pswd))
            {
                user.user_pswd = BCrypt.Net.BCrypt.HashPassword(dto.user_pswd);
            }

            await _db.SaveChangesAsync();

            Console.WriteLine($"[Users] Utilisateur {id} modifié");

            return Ok(new
            {
                user.user_id,
                user.user_name,
                user.user_mail,
                user.role_id
            });
        }

        // DELETE /users/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _db.USERS.FindAsync(id);
            if (user == null) return NotFound();

            _db.USERS.Remove(user);
            await _db.SaveChangesAsync();

            Console.WriteLine($"[Users] Utilisateur {id} supprimé");

            return NoContent();
        }

        [HttpGet("due-reminders")]
        public async Task<IActionResult> GetDueReminders()
        {
            var userId = HttpContext.Session.GetInt32("user_id");
            if (!userId.HasValue) return Unauthorized();

            var today = DateTime.Today;
            var thirtyDaysFromNow = today.AddDays(30);

            var dueSoon = await _db.BORROWED
                .Where(b => b.user_id == userId.Value
                         && !b.is_returned
                         && b.date_end >= today
                         && b.date_end <= thirtyDaysFromNow)
                .Include(b => b.Book)
                .Select(b => new
                {
                    b.id_borrow,
                    BookName = b.Book.book_name,
                    b.date_end,
                    DaysLeft = EF.Functions.DateDiffDay(today, b.date_end)
                })
                .OrderBy(b => b.date_end)
                .ToListAsync();

            bool hasCritical = dueSoon.Any(b => b.DaysLeft <= 5);
            string message = hasCritical
                ? "⚠️ Urgence : un ou plusieurs livres à rendre dans 5 jours ou moins !"
                : "⏰ Rappel : un ou plusieurs livres à rendre dans moins de 30 jours.";

            return Ok(new
            {
                hasReminder = dueSoon.Any(),
                isCritical = hasCritical,
                message,
                details = dueSoon.Select(d => new
                {
                    d.id_borrow,
                    bookName = d.BookName,
                    date_end = d.date_end,
                    daysLeft = d.DaysLeft
                })
            });
        }
    }
}