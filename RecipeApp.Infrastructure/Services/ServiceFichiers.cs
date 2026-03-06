using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using RecipeApp.Application.Interfaces;

namespace RecipeApp.Infrastructure.Services;

/// <summary>Service pour la sauvegarde locale des images uploadées.</summary>
public class ServiceFichiers : IServiceFichiers
{
    private readonly string _dossierUploads;
    private readonly ILogger<ServiceFichiers> _logger;

    public ServiceFichiers(IWebHostEnvironment env, ILogger<ServiceFichiers> logger)
    {
        _dossierUploads = Path.Combine(env.WebRootPath, "uploads", "recettes");
        _logger = logger;

        // Créer le dossier s'il n'existe pas
        Directory.CreateDirectory(_dossierUploads);
    }

    public async Task<string> SauvegarderImageAsync(string imageBase64, string typeMime, CancellationToken annulation = default)
    {
        var extension = typeMime switch
        {
            "image/jpeg" or "image/jpg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            _ => ".jpg"
        };

        var nomFichier = $"{Guid.NewGuid()}{extension}";
        var cheminComplet = Path.Combine(_dossierUploads, nomFichier);

        var octets = Convert.FromBase64String(imageBase64);
        await File.WriteAllBytesAsync(cheminComplet, octets, annulation);

        _logger.LogInformation("Image sauvegardée : {Chemin}", cheminComplet);

        // Retourner le chemin relatif pour l'URL
        return $"/uploads/recettes/{nomFichier}";
    }

    public Task SupprimerImageAsync(string cheminImage, CancellationToken annulation = default)
    {
        try
        {
            // Convertir le chemin URL en chemin physique
            var nomFichier = Path.GetFileName(cheminImage);
            var cheminComplet = Path.Combine(_dossierUploads, nomFichier);

            if (File.Exists(cheminComplet))
            {
                File.Delete(cheminComplet);
                _logger.LogInformation("Image supprimée : {Chemin}", cheminComplet);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression de l'image : {Chemin}", cheminImage);
        }

        return Task.CompletedTask;
    }
}
