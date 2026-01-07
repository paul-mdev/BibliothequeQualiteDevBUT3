using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BibliothequeQualiteDev.Server.Models;

using System.Data;

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
    public async Task<ActionResult<IEnumerable<RolesModel>>> GetRoles()
    {
        var roles = await _db.ROLES.ToListAsync();
        return Ok(roles);
    }

    // GET /roles/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<RolesModel>> GetRole(int id)
    {
        var role = await _db.ROLES.FindAsync(id);
        if (role == null) return NotFound();
        return Ok(role);
    }
}
