using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RecipeApp.Application.Interfaces;
using RecipeApp.Domain.Interfaces;
using RecipeApp.Infrastructure.Identity;
using RecipeApp.Infrastructure.Persistence;
using RecipeApp.Infrastructure.Repositories;
using RecipeApp.Infrastructure.Services;

namespace RecipeApp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AjouterCoucheInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var chaineConnexion = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Chaîne de connexion 'DefaultConnection' manquante.");

        // Factory pour Blazor Server — chaque opération crée son propre DbContext
        services.AddDbContextFactory<AppDbContext>(options =>
            options.UseMySql(chaineConnexion, ServerVersion.AutoDetect(chaineConnexion)));

        // Contexte scoped pour ASP.NET Identity (SignInManager, UserManager)
        services.AddScoped<AppDbContext>(sp =>
            sp.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());

        // ASP.NET Identity
        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 6;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.SignIn.RequireConfirmedEmail = false;
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        // Repositories de lecture — contexte frais par méthode (thread-safe Blazor Server)
        services.AddScoped<IRecetteRepository, RecetteQueryRepository>();
        services.AddScoped<ICategorieRepository, CategorieRepository>();
        services.AddScoped<IEtiquetteRepository, EtiquetteRepository>();

        // Unité de travail — contexte frais par commande (écriture atomique)
        services.AddTransient<IUnitesDeTravail, UnitesDeTravail>();

        // Services applicatifs
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IServiceFichiers, ServiceFichiers>();
        services.AddHttpClient<IServiceVision, ServiceVision>();

        return services;
    }
}
