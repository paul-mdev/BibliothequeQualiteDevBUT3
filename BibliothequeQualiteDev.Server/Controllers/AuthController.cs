using Microsoft.AspNetCore.Mvc;

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
    public IActionResult Register([FromBody] UserModel user)
    {
        if (_db.USER.Any(u => u.user_mail == user.user_mail))
            return BadRequest("Email already exists");

        user.role_id = 1; // rôle par défaut
        _db.USER.Add(user);
        _db.SaveChanges();

        // session
        HttpContext.Session.SetInt32("user_id", user.user_id);
        HttpContext.Session.SetString("user_mail", user.user_mail);

        return Ok(new { user.user_id, user.user_mail });
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] UserModel login)
    {
        var user = _db.USER.FirstOrDefault(u => u.user_mail == login.user_mail && u.user_pswd == login.user_pswd);
        if (user == null) return Unauthorized("Invalid email or password");

        HttpContext.Session.SetInt32("user_id", user.user_id);
        HttpContext.Session.SetString("user_mail", user.user_mail);

        return Ok(new { user.user_id, user.user_mail });
    }

    [HttpGet("me")]
    public IActionResult Me()
    {
        var id = HttpContext.Session.GetInt32("user_id");
        var email = HttpContext.Session.GetString("user_mail");
        if (id == null) return Unauthorized();
        return Ok(new { user_id = id, user_mail = email });
    }


    [HttpPost("logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return Ok();
    }
}
