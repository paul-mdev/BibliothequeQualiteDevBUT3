using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BibliothequeQualiteDev.Server.Models;

/// <summary>
/// ===== CONTRÔLEUR DE GESTION DES RÔLES =====
/// Fournit la liste des rôles disponibles
/// Route de base : /roles
/// Lecture seule (pas de création/modification de rôles via l'API)
/// </summary>
[ApiController]
[Route("[controller]")]
public class RolesController : ControllerBase
{
    private readonly AppDbContext _db;

    public RolesController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// ===== GET /roles =====
    /// Liste tous les rôles disponibles
    /// Utilisé pour remplir le select dans le formulaire utilisateur
    /// </summary>
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

    /// <summary>
    /// ===== GET /roles/{id} =====
    /// Récupère un rôle spécifique
    /// Peu utilisé dans l'application actuelle
    /// </summary>
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