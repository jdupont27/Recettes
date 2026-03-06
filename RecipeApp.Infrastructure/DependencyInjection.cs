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
        // Base de données MySQL avec EF Core
        var chaineConnexion = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Chaîne de connexion 'DefaultConnection' manquante.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseMySql(chaineConnexion, ServerVersion.AutoDetect(chaineConnexion)));

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

        // Repositories
        services.AddScoped<IRecetteRepository, RecetteRepository>();
        services.AddScoped<ICategorieRepository, CategorieRepository>();
        services.AddScoped<IEtiquetteRepository, EtiquetteRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Services applicatifs
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IServiceFichiers, ServiceFichiers>();
        services.AddHttpClient<IServiceVision, ServiceVision>();

        return services;
    }
}
