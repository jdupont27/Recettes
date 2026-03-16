using Amazon;
using Amazon.S3;
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
            options.UseMySql(chaineConnexion, ServerVersion.AutoDetect(chaineConnexion),
                mysqlOptions => mysqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null)));

        // Contexte scoped pour ASP.NET Identity (SignInManager, UserManager)
        services.AddScoped<AppDbContext>(sp =>
            sp.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());

        // ASP.NET Identity
        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 12;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = true;
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

        var bucketName = configuration["AWS:BucketName"];
        if (!string.IsNullOrEmpty(bucketName))
        {
            var region = configuration["AWS:Region"] ?? "us-east-1";
            services.AddSingleton<IAmazonS3>(_ => new AmazonS3Client(RegionEndpoint.GetBySystemName(region)));
            services.AddScoped<IServiceFichiers, ServiceFichiersS3>();
        }
        else
        {
            services.AddScoped<IServiceFichiers, ServiceFichiers>();
        }

        services.AddHttpClient<IServiceVision, ServiceVision>();

        return services;
    }
}
