using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BibliothequeQualiteDev.Server.Models;

namespace BibliothequeQualiteDev.Server.Controllers
{
    /// <summary>
    /// ===== CONTRÔLEUR DE GESTION DES UTILISATEURS =====
    /// CRUD complet sur les utilisateurs (pour les administrateurs)
    /// Route de base : /users
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _db;

        public UsersController(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// DTO pour création/modification d'utilisateur
        /// </summary>
        public class UsersCreateDTO
        {
            public string user_name { get; set; } = string.Empty;
            public string user_mail { get; set; } = string.Empty;
            public string user_pswd { get; set; } = string.Empty;
            public int role_id { get; set; }
        }

        /// <summary>
        /// ===== GET /users =====
        /// Liste tous les utilisateurs avec leur rôle
        /// Utilisé pour la page de gestion des utilisateurs
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _db.USERS
                .Include(u => u.role)  // Charge le rôle de chaque utilisateur
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

        /// <summary>
        /// ===== GET /users/{id} =====
        /// Récupère un utilisateur spécifique
        /// Utilisé pour charger les données dans le formulaire d'édition
        /// </summary>
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

        /// <summary>
        /// ===== POST /users =====
        /// Crée un nouvel utilisateur (par un administrateur)
        /// Différent de /auth/register car permet de choisir le rôle
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] UsersCreateDTO dto)
        {
            // ===== VALIDATION =====
            if (string.IsNullOrEmpty(dto.user_pswd))
                return BadRequest("Mot de passe requis");

            if (_db.USERS.Any(u => u.user_mail == dto.user_mail))
                return BadRequest("Email déjà utilisé");

            // ===== CRÉATION =====
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

        /// <summary>
        /// ===== PUT /users/{id} =====
        /// Modifie un utilisateur existant
        /// Le mot de passe n'est re-hashé que s'il est fourni
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UsersCreateDTO dto)
        {
            var user = await _db.USERS.FindAsync(id);
            if (user == null) return NotFound();

            // ===== MISE À JOUR DES CHAMPS =====
            user.user_name = dto.user_name;
            user.user_mail = dto.user_mail;
            user.role_id = dto.role_id;

            // ===== MOT DE PASSE OPTIONNEL =====
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

        /// <summary>
        /// ===== DELETE /users/{id} =====
        /// Supprime un utilisateur
        /// ATTENTION : Peut supprimer les emprunts associés selon la config cascade
        /// </summary>
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
    }
}
