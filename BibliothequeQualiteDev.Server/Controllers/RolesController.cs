using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BibliothequeQualiteDev.Server.Models;

[ApiController]
[Route("[controller]")]
public class RolesController : ControllerBase
{
    private readonly AppDbContext _db;

    public RolesController(AppDbContext db)
    {
        _db = db;
    }

    // GET /roles
    [HttpGet]
    public async Task<ActionResult> GetRoles()
    {
        var roles = await _db.ROLES
            .Select(r => new {
                r.role_id,
                r.role_name
            })
            .ToListAsync();

        Console.WriteLine($"[Roles] Retour de {roles.Count} rôles");
        return Ok(roles);
    }

    // GET /roles/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult> GetRole(int id)
    {
        var role = await _db.ROLES
            .Where(r => r.role_id == id)
            .Select(r => new {
                r.role_id,
                r.role_name
            })
            .FirstOrDefaultAsync();

        if (role == null) return NotFound();
        return Ok(role);
    }
}