using Microsoft.AspNetCore.Mvc;
using BibliothequeQualiteDev.Server.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

/// <summary>
/// ===== CONTRÔLEUR D'AUTHENTIFICATION =====
/// Gère l'inscription, la connexion et la déconnexion des utilisateurs
/// Route de base : /auth
/// 
/// Sécurité :
/// - Mots de passe hashés avec BCrypt
/// - Sessions serveur pour maintenir l'état de connexion
/// - Validation des credentials
/// </summary>
[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;

    public AuthController(AppDbContext db)
    {
        _db = db;
    }

    // ===== DTOs POUR L'AUTHENTIFICATION =====

    /// <summary>
    /// DTO pour l'inscription
    /// </summary>
    public class RegisterDTO
    {
        public string user_name { get; set; } = string.Empty;
        public string user_mail { get; set; } = string.Empty;
        public string user_pswd { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO pour la connexion
    /// </summary>
    public class LoginDTO
    {
        public string user_mail { get; set; } = string.Empty;
        public string user_pswd { get; set; } = string.Empty;
    }

    /// <summary>
    /// ===== POST /auth/register =====
    /// Inscription d'un nouvel utilisateur
    /// 
    /// Processus :
    /// 1. Vérification de l'unicité de l'email
    /// 2. Hashage du mot de passe avec BCrypt
    /// 3. Création de l'utilisateur avec le rôle "Étudiant" (role_id = 3)
    /// 4. Création automatique de la session
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDTO dto)
    {
        // ===== VÉRIFICATION UNICITÉ EMAIL =====
        if (_db.USERS.Any(u => u.user_mail == dto.user_mail))
            return BadRequest("Email déjà utilisé");

        // ===== CRÉATION DE L'UTILISATEUR =====
        var user = new UsersModel
        {
            user_name = dto.user_name,
            user_mail = dto.user_mail,
            // ===== SÉCURITÉ : Hashage du mot de passe =====
            // BCrypt génère automatiquement un salt unique
            user_pswd = BCrypt.Net.BCrypt.HashPassword(dto.user_pswd),
            role_id = 3 // Rôle "Étudiant" par défaut
        };

        _db.USERS.Add(user);
        await _db.SaveChangesAsync();

        // ===== CRÉATION DE LA SESSION =====
        // Stocke l'ID et l'email dans la session côté serveur
        HttpContext.Session.SetInt32("user_id", user.user_id);
        HttpContext.Session.SetString("user_mail", user.user_mail);

        return Ok(new { user.user_id, user.user_mail });
    }

    /// <summary>
    /// ===== POST /auth/login =====
    /// Connexion d'un utilisateur existant
    /// 
    /// Vérifications :
    /// 1. Existence de l'email
    /// 2. Vérification du mot de passe hashé
    /// 3. Création de la session si succès
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO dto)
    {
        // ===== RECHERCHE DE L'UTILISATEUR =====
        // Include(u => u.role) charge également le rôle associé
        var user = await _db.USERS
            .Include(u => u.role)
            .FirstOrDefaultAsync(u => u.user_mail == dto.user_mail);

        if (user == null)
            return Unauthorized("Email ou mot de passe incorrect");

        // ===== VÉRIFICATION DU MOT DE PASSE =====
        // BCrypt.Verify compare le mot de passe en clair avec le hash
        if (!BCrypt.Net.BCrypt.Verify(dto.user_pswd, user.user_pswd))
            return Unauthorized("Email ou mot de passe incorrect");

        // ===== CRÉATION DE LA SESSION =====
        HttpContext.Session.SetInt32("user_id", user.user_id);
        HttpContext.Session.SetString("user_mail", user.user_mail);

        // ===== RETOUR DES INFORMATIONS =====
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

    /// <summary>
    /// ===== GET /auth/me =====
    /// Récupère les informations de l'utilisateur connecté
    /// Endpoint crucial pour :
    /// - Vérifier l'authentification
    /// - Récupérer les droits de l'utilisateur
    /// - Afficher les infos dans le frontend
    /// 
    /// Charge les relations en profondeur :
    /// User → Role → RoleRights → Rights
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        // ===== VÉRIFICATION SESSION =====
        var userId = HttpContext.Session.GetInt32("user_id");
        if (userId == null) return Unauthorized();

        // ===== CHARGEMENT DE L'UTILISATEUR AVEC TOUTES SES RELATIONS =====
        var user = await _db.USERS
            .Include(u => u.role)                        // Charge le rôle
            .ThenInclude(r => r.role_rights)             // Puis les role_rights
            .ThenInclude(rr => rr.right)                 // Puis les rights
            .FirstOrDefaultAsync(u => u.user_id == userId);

        if (user == null) return Unauthorized();

        // ===== LOGS DEBUG =====
        // Utiles pour déboguer les problèmes de droits
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

        // ===== EXTRACTION DE LA LISTE DES DROITS =====
        // Crée une liste simple de noms de droits pour le frontend
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
                rights = rights // IMPORTANT : retourne la liste des droits
            }
        });
    }

    /// <summary>
    /// ===== POST /auth/logout =====
    /// Déconnexion de l'utilisateur
    /// Efface toutes les données de session
    /// </summary>
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return Ok();
    }
}
