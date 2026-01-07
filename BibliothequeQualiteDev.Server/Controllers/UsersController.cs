using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BibliothequeQualiteDev.Server.Models;

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

        // DTO utilisé pour CREATE et UPDATE
        public class UsersDTO
        {
            public string user_name { get; set; } = string.Empty;
            public string user_mail { get; set; } = string.Empty;
            public string? user_pswd { get; set; }
            public int role_id { get; set; }
        }

        // GET /users
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _db.USERS
                .Include(u => u.role)
                .ToListAsync();

            return Ok(users);
        }

        // GET /users/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _db.USERS
                .Include(u => u.role)
                .FirstOrDefaultAsync(u => u.user_id == id);

            if (user == null) return NotFound();
            return Ok(user);
        }

        // POST /users
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] UsersDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.user_pswd))
                return BadRequest("Mot de passe requis");

            var user = new UsersModel
            {
                user_name = dto.user_name,
                user_mail = dto.user_mail,
                user_pswd = dto.user_pswd, // hash si tu veux plus tard
                role_id = dto.role_id
            };

            _db.USERS.Add(user);
            await _db.SaveChangesAsync();

            return Ok(user);
        }

        // PUT /users/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UsersDTO dto)
        {
            var user = await _db.USERS.FindAsync(id);
            if (user == null) return NotFound();

            user.user_name = dto.user_name;
            user.user_mail = dto.user_mail;
            user.role_id = dto.role_id;

            // ⚠️ IMPORTANT : ne pas écraser le mot de passe s’il est vide
            if (!string.IsNullOrWhiteSpace(dto.user_pswd))
            {
                user.user_pswd = dto.user_pswd;
            }

            await _db.SaveChangesAsync();
            return Ok(user);
        }

        // DELETE /users/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _db.USERS.FindAsync(id);
            if (user == null) return NotFound();

            _db.USERS.Remove(user);
            await _db.SaveChangesAsync();
            return Ok();
        }
    }
}
