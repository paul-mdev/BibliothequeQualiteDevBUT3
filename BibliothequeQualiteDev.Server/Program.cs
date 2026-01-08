
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// ===== POINT D'ENTRÉE DE L'APPLICATION =====
/// Configure tous les services et le pipeline de traitement des requêtes
/// 
/// Configuration principale :
/// - Base de données MySQL avec Entity Framework Core
/// - Sessions serveur pour l'authentification
/// - CORS pour le frontend Vue.js
/// - Fichiers statiques pour les images
/// </summary>

var builder = WebApplication.CreateBuilder(args);

// ========================================
// CONFIGURATION DES SERVICES
// ========================================

// ===== CONTRÔLEURS API =====
// Active le support des contrôleurs Web API
builder.Services.AddControllers();

// ===== BASE DE DONNÉES =====
// Configuration de Entity Framework Core avec MySQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        // Récupère la chaîne de connexion depuis appsettings.json
        builder.Configuration.GetConnectionString("DefaultConnection"),
        // Spécifie la version de MySQL (8.0.34)
        new MySqlServerVersion(new Version(8, 0, 34))
    )
);

// ===== SESSIONS =====
// Configuration de la gestion de session pour l'authentification
builder.Services.AddDistributedMemoryCache();  // Stockage en mémoire
builder.Services.AddSession(options =>
{
    // Durée de vie de la session : 1 heure d'inactivité
    options.IdleTimeout = TimeSpan.FromHours(1);

    // ===== SÉCURITÉ DES COOKIES =====
    options.Cookie.HttpOnly = true;  // Protège contre XSS
    options.Cookie.IsEssential = true;  // Cookie essentiel au fonctionnement

    // IMPORTANT : Configuration pour HTTPS et cross-origin
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;  // HTTPS uniquement
    options.Cookie.SameSite = SameSiteMode.None;  // Permet cross-origin avec credentials
});

// ===== CORS (Cross-Origin Resource Sharing) =====
// Permet au frontend Vite (port 5173) d'accéder à l'API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        // Origine autorisée : le serveur de dev Vite
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()        // Tous les headers autorisés
              .AllowAnyMethod()        // Toutes les méthodes HTTP (GET, POST, etc.)
              .AllowCredentials();     // CRITIQUE : Permet l'envoi des cookies
    });
});

// ===== HEALTH CHECKS =====
// Endpoint de santé pour monitoring (/health)
builder.Services.AddHealthChecks();

// ===== DATA PROTECTION =====
// Stockage des clés de chiffrement sur disque
// Important pour la persistance des sessions après redémarrage
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(@"/root/.aspnet/DataProtection-Keys"));

// ========================================
// CONSTRUCTION DE L'APPLICATION
// ========================================

var app = builder.Build();

// ===== HEALTH CHECK ENDPOINT =====
// Expose /health pour vérifier que l'API fonctionne
app.MapHealthChecks("/health");

// ===== REDIRECTION HTTPS =====
// En production, force l'utilisation de HTTPS
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// ===== FICHIERS STATIQUES =====
// Sert les fichiers du dossier wwwroot (images, etc.)
app.UseStaticFiles();

// ===== ROUTING =====
// Active le système de routage
app.UseRouting();

// ========================================
// PIPELINE DE TRAITEMENT DES REQUÊTES
// ========================================
// ORDRE CRUCIAL : CORS → Session → Authorization

// 1. CORS : Gère les headers cross-origin
app.UseCors("AllowFrontend");

// 2. SESSION : Active la gestion de session
// IMPORTANT : Doit être AVANT UseAuthorization
app.UseSession();

// 3. AUTHORIZATION : Gère les autorisations
app.UseAuthorization();

// ===== MAPPING DES CONTRÔLEURS =====
// Scanne et mappe tous les contrôleurs de l'application
app.MapControllers();

// ===== FALLBACK POUR SPA =====
// Redirige toutes les routes non trouvées vers index.html
// Permet le routing côté client (Vue Router)
app.MapFallbackToFile("/index.html");

// ===== DÉMARRAGE DE L'APPLICATION =====
app.Run();