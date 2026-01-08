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

    [HttpPost("register")]
    public IActionResult Register([FromBody] UsersModel user)
    {
        if (_db.USERS.Any(u => u.user_mail == user.user_mail))
            return BadRequest("Email already exists");

        user.role_id = 3; // rôle étudiant par défaut
        user.user_pswd = BCrypt.Net.BCrypt.HashPassword(user.user_pswd); // ← Hash le mot de passe

        _db.USERS.Add(user);
        _db.SaveChanges();

        HttpContext.Session.SetInt32("user_id", user.user_id);
        HttpContext.Session.SetString("user_mail", user.user_mail);
        return Ok(new { user.user_id, user.user_mail });
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] UsersModel login)
    {
        var user = _db.USERS.FirstOrDefault(u => u.user_mail == login.user_mail);

        // ← Vérification avec bcrypt
        if (user == null || !BCrypt.Net.BCrypt.Verify(login.user_pswd, user.user_pswd))
            return Unauthorized("Invalid email or password");

        HttpContext.Session.SetInt32("user_id", user.user_id);
        HttpContext.Session.SetString("user_mail", user.user_mail);
        return Ok(new { user.user_id, user.user_mail });
    }

    public class UserMeDto
    {
        public int user_id { get; set; }
        public string user_mail { get; set; }
        public string role_name { get; set; }
        public List<string> rights { get; set; }
       
    }
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userId = HttpContext.Session.GetInt32("user_id");
        if (userId == null) return Unauthorized();


        var user = await _db.USERS
            .Include(u => u.role)          // charge le rôle
            .ThenInclude(r => r.role_rights) // charge les droits du rôle
            .ThenInclude(rr => rr.right)
            .FirstOrDefaultAsync(u => u.user_id == userId);

        if (user == null) return Unauthorized();

        return Ok(new
        {
            user.user_id,
            user.user_mail,
            user.user_name,
            role = new
            {
                user.role.role_id,
                user.role.role_name,
                rights = user.role.role_rights.Select(rr => rr.right.right_name).ToList()
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
