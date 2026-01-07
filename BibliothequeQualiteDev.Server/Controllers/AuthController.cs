using Microsoft.AspNetCore.Mvc;
using BibliothequeQualiteDev.Server.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    public AuthController(AppDbContext db)
    {
        _db = db;
    }

    public class RegisterDTO
    {
        public string user_name { get; set; } = string.Empty;
        public string user_mail { get; set; } = string.Empty;
        public string user_pswd { get; set; } = string.Empty;
    }

    public class LoginDTO
    {
        public string user_mail { get; set; } = string.Empty;
        public string user_pswd { get; set; } = string.Empty;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDTO dto)
    {
        if (_db.USERS.Any(u => u.user_mail == dto.user_mail))
            return BadRequest("Email déjà utilisé");

        var user = new UsersModel
        {
            user_name = dto.user_name,
            user_mail = dto.user_mail,
            user_pswd = BCrypt.Net.BCrypt.HashPassword(dto.user_pswd),
            role_id = 3 // Étudiant par défaut
        };

        _db.USERS.Add(user);
        await _db.SaveChangesAsync();

        // Session
        HttpContext.Session.SetInt32("user_id", user.user_id);
        HttpContext.Session.SetString("user_mail", user.user_mail);

        return Ok(new { user.user_id, user.user_mail });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO dto)
    {
        var user = await _db.USERS
            .Include(u => u.role)
            .FirstOrDefaultAsync(u => u.user_mail == dto.user_mail);

        if (user == null)
            return Unauthorized("Email ou mot de passe incorrect");

        // Vérifier le mot de passe hashé
        if (!BCrypt.Net.BCrypt.Verify(dto.user_pswd, user.user_pswd))
            return Unauthorized("Email ou mot de passe incorrect");

        // Session
        HttpContext.Session.SetInt32("user_id", user.user_id);
        HttpContext.Session.SetString("user_mail", user.user_mail);

        return Ok(new
        {
            user.user_id,
            user.user_mail,
            user.user_name,
            role = new
            {
                user.role.role_id,
                user.role.role_name
            }
        });
    }

    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userId = HttpContext.Session.GetInt32("user_id");
        if (userId == null) return Unauthorized();

        var user = await _db.USERS
            .Include(u => u.role)
            .ThenInclude(r => r.role_rights)
            .ThenInclude(rr => rr.right)
            .FirstOrDefaultAsync(u => u.user_id == userId);

        if (user == null) return Unauthorized();

        // ⭐ DEBUG : Log pour voir ce qui est chargé
        Console.WriteLine($"[AUTH/ME] User: {user.user_name}");
        Console.WriteLine($"[AUTH/ME] Role: {user.role?.role_name}");
        Console.WriteLine($"[AUTH/ME] Role_rights count: {user.role?.role_rights?.Count}");

        if (user.role?.role_rights != null)
        {
            foreach (var rr in user.role.role_rights)
            {
                Console.WriteLine($"[AUTH/ME] Right: {rr.right?.right_name}");
            }
        }

        // ⭐ Créer la liste des droits
        var rights = user.role?.role_rights?
            .Select(rr => rr.right.right_name)
            .ToList() ?? new List<string>();

        Console.WriteLine($"[AUTH/ME] Rights list: {string.Join(", ", rights)}");

        return Ok(new
        {
            user.user_id,
            user.user_mail,
            user.user_name,
            role = new
            {
                user.role.role_id,
                user.role.role_name,
                rights = rights // ⭐ IMPORTANT : retourner directement la liste
            }
        });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return Ok();
    }
}