using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RecipeApp.Application.Interfaces;

namespace RecipeApp.Infrastructure.Services;

/// <summary>Service de stockage d'images sur Amazon S3.</summary>
public class ServiceFichiersS3 : IServiceFichiers
{
    private readonly IAmazonS3 _s3;
    private readonly string _bucketName;
    private readonly ILogger<ServiceFichiersS3> _logger;

    public ServiceFichiersS3(IAmazonS3 s3, IConfiguration configuration, ILogger<ServiceFichiersS3> logger)
    {
        _s3 = s3;
        _bucketName = configuration["AWS:BucketName"] ?? throw new InvalidOperationException("AWS:BucketName manquant.");
        _logger = logger;
    }

    public async Task<string> SauvegarderImageAsync(string imageBase64, string typeMime, CancellationToken annulation = default)
    {
        var extension = typeMime switch
        {
            "image/png" => "png",
            "image/gif" => "gif",
            "image/webp" => "webp",
            _ => "jpg"
        };

        var cle = $"recettes/{Guid.NewGuid()}.{extension}";
        var octets = Convert.FromBase64String(imageBase64);

        using var flux = new MemoryStream(octets);

        var requete = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = cle,
            InputStream = flux,
            ContentType = typeMime
        };

        await _s3.PutObjectAsync(requete, annulation);

        var url = $"https://{_bucketName}.s3.amazonaws.com/{cle}";
        _logger.LogInformation("Image sauvegardée sur S3 : {Url}", url);
        return url;
    }

    public async Task SupprimerImageAsync(string cheminImage, CancellationToken annulation = default)
    {
        if (!cheminImage.StartsWith("https://"))
            return; // Chemin local — rien à supprimer sur S3

        try
        {
            var uri = new Uri(cheminImage);
            var cle = uri.AbsolutePath.TrimStart('/');

            await _s3.DeleteObjectAsync(_bucketName, cle, annulation);
            _logger.LogInformation("Image supprimée de S3 : {Cle}", cle);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Impossible de supprimer l'image S3 : {Chemin}", cheminImage);
        }
    }
}
