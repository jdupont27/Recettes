using RecipeApp.Application;
using RecipeApp.Infrastructure;
using RecipeApp.Infrastructure.Persistence;
using RecipeApp.Web.Components;
using RecipeApp.Web.Middleware;
using Serilog;

// Configuration initiale de Serilog (bootstrap logger)
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Serilog : lecture depuis appsettings + écriture console et fichier
    builder.Host.UseSerilog((contexte, services, config) => config
        .ReadFrom.Configuration(contexte.Configuration)
        .ReadFrom.Services(services)
        .WriteTo.Console()
        .WriteTo.File("logs/recetteapp-.log", rollingInterval: RollingInterval.Day));

    // Services Blazor Server avec composants interactifs
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();

    // Rend l'état d'authentification disponible comme paramètre en cascade (statique + interactif)
    builder.Services.AddCascadingAuthenticationState();

    // Couches Application et Infrastructure
    builder.Services.AjouterCoucheApplication();
    builder.Services.AjouterCoucheInfrastructure(builder.Configuration);

    // Contrôleurs pour l'API Vision
    builder.Services.AddControllers();

    // Accès au HttpContext (nécessaire pour CurrentUserService)
    builder.Services.AddHttpContextAccessor();

    // Configuration des cookies d'authentification
    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.LoginPath = "/compte/connexion";
        options.LogoutPath = "/compte/deconnexion";
        options.AccessDeniedPath = "/acces-refuse";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });

    var app = builder.Build();

    // Middleware global d'erreurs
    app.UseMiddleware<GestionErreurs>();

    if (!app.Environment.IsDevelopment())
    {
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseAntiforgery();

    // Ordre important : Authentication avant Authorization
    app.UseAuthentication();
    app.UseAuthorization();

    // Endpoints API REST
    app.MapControllers();

    // Composants Blazor Server
    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

    // Initialiser la base de données et les données de référence
    using (var scope = app.Services.CreateScope())
    {
        var contexte = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await contexte.Database.EnsureCreatedAsync();
        await InitialiseurDonnees.InitialiserAsync(scope.ServiceProvider);
    }

    Log.Information("Application RecetteApp démarrée.");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "L'application a échoué au démarrage.");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
