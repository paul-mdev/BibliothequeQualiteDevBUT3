using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 34))
    )
);

// Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Important pour SameSite=None en HTTPS
    options.Cookie.SameSite = SameSiteMode.None; // Permet l'envoi cross-site avec credentials: include
});


// CORS pour Vite
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});


var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseCors("AllowFrontend"); // <-- autoriser le front
app.UseSession();             // <-- session avant l'authorization
app.UseAuthorization();

app.MapControllers();
app.MapFallbackToFile("/index.html");

app.Run();
using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

// Appliquer les migrations automatiquement (optionnel mais pratique en dev)
db.Database.Migrate();

// Seeding d'un admin si la table est vide
if (!await db.USER.AnyAsync())
{
    var hashedPassword = "admin123";

    await db.USER.AddAsync(new UsersModel
    {
        user_name = "Administrateur",
        user_mail = "admin@bibliotheque.com",
        user_pswd = hashedPassword,
        role_id = 1 // Assume que 1 = rôle admin
    });

    await db.SaveChangesAsync();
}