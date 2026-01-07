using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net; // Install-Package BCrypt.Net-Next

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;

    public UsersController(AppDbContext db)
    {
        _db = db;
    }

    // GET /users
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UsersModel>>> GetUsers()
    {
        var users = await _db.USER.ToListAsync();
        return Ok(users);
    }

    // GET /users/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<UsersModel>> GetUser(int id)
    {
        var user = await _db.USER.FindAsync(id);
        if (user == null) return NotFound();
        return Ok(user);
    }

    // POST /users
    [HttpPost]
    public async Task<ActionResult<UsersModel>> CreateUser([FromBody] UsersModel user)
    {
        if (string.IsNullOrEmpty(user.user_pswd))
            return BadRequest("Mot de passe requis");

        // Hash du mot de passe
        user.user_pswd = BCrypt.Net.BCrypt.HashPassword(user.user_pswd);

        _db.USER.Add(user);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetUser), new { id = user.user_id }, user);
    }

    // PUT /users/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UsersModel updatedUser)
    {
        var user = await _db.USER.FindAsync(id);
        if (user == null) return NotFound();

        user.user_name = updatedUser.user_name;
        user.user_mail = updatedUser.user_mail;
        user.role_id = updatedUser.role_id;

        // Si mot de passe fourni -> rehash
        if (!string.IsNullOrEmpty(updatedUser.user_pswd))
        {
            user.user_pswd = BCrypt.Net.BCrypt.HashPassword(updatedUser.user_pswd);
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE /users/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _db.USER.FindAsync(id);
        if (user == null) return NotFound();

        _db.USER.Remove(user);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
