using System.Text.RegularExpressions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RecipeApp.Application.DTOs;
using RecipeApp.Application.Vision.Commands.ExtraireIngredients;
using RecipeApp.Application.Vision.Commands.ExtraireVideo;

namespace RecipeApp.Web.Controllers;

/// <summary>API d'extraction de recette par photo ou vidéo via Gemini.</summary>
[ApiController]
[Route("api/vision")]
[Authorize]
public class VisionController : ControllerBase
{
    private static readonly Regex RegexYoutube = new(
        @"^https?://(www\.)?(youtube\.com/(watch\?v=|shorts/)|youtu\.be/)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private readonly IMediator _mediateur;
    private readonly IConfiguration _configuration;

    public VisionController(IMediator mediateur, IConfiguration configuration)
    {
        _mediateur = mediateur;
        _configuration = configuration;
    }

    /// <summary>
    /// Extrait la recette complète depuis une image en base64.
    /// POST /api/vision/extraire-ingredients
    /// Corps : { "imageBase64": "...", "typeMime": "image/jpeg" }
    /// </summary>
    [HttpPost("extraire-ingredients")]
    [EnableRateLimiting("vision")]
    public async Task<ActionResult<RecetteExtraiteDto>> ExtraireIngredients(
        [FromBody] RequeteExtractionIngredients requete,
        CancellationToken annulation)
    {
        if (string.IsNullOrEmpty(requete.ImageBase64))
            return BadRequest(new { erreur = "L'image est obligatoire." });

        string[] mimeAutorises = ["image/jpeg", "image/png", "image/webp", "image/gif"];
        var typeMime = requete.TypeMime ?? "image/jpeg";
        if (!mimeAutorises.Contains(typeMime))
            return BadRequest(new { erreur = "Type de fichier non autorisé." });

        var commande = new ExtraireIngredientsCommand
        {
            ImageBase64 = requete.ImageBase64,
            TypeMime = typeMime
        };

        var recette = await _mediateur.Send(commande, annulation);
        return Ok(recette);
    }

    /// <summary>
    /// Extrait la recette complète depuis une vidéo de cuisine : soit un fichier uploadé, soit une URL YouTube.
    /// POST /api/vision/extraire-video (multipart/form-data)
    /// Champs : "video" (fichier) OU "urlYoutube" (chaîne) — exactement un des deux.
    /// </summary>
    [HttpPost("extraire-video")]
    [EnableRateLimiting("vision-video")]
    [RequestSizeLimit(210_000_000)]
    [RequestFormLimits(MultipartBodyLengthLimit = 210_000_000)]
    public async Task<ActionResult<RecetteExtraiteDto>> ExtraireVideo(
        [FromForm] IFormFile? video,
        [FromForm] string? urlYoutube,
        CancellationToken annulation)
    {
        if (video == null && string.IsNullOrWhiteSpace(urlYoutube))
            return BadRequest(new { erreur = "Fournissez soit un fichier vidéo, soit une URL YouTube." });

        if (video != null && !string.IsNullOrWhiteSpace(urlYoutube))
            return BadRequest(new { erreur = "Fournissez soit un fichier vidéo, soit une URL YouTube, pas les deux." });

        if (!string.IsNullOrWhiteSpace(urlYoutube))
        {
            if (urlYoutube.Contains("tiktok.com", StringComparison.OrdinalIgnoreCase) ||
                urlYoutube.Contains("instagram.com", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { erreur = "Les liens TikTok et Instagram Reels ne sont pas pris en charge pour l'instant. Téléchargez la vidéo et importez-la comme fichier." });
            }

            if (!RegexYoutube.IsMatch(urlYoutube))
                return BadRequest(new { erreur = "L'URL doit être un lien YouTube valide (youtube.com/watch, youtube.com/shorts ou youtu.be)." });

            var recetteYoutube = await _mediateur.Send(new ExtraireVideoCommand { UrlYoutube = urlYoutube }, annulation);
            return Ok(recetteYoutube);
        }

        string[] mimeVideoAutorises = ["video/mp4", "video/quicktime", "video/webm", "video/x-msvideo"];
        if (!mimeVideoAutorises.Contains(video!.ContentType))
            return BadRequest(new { erreur = "Type de vidéo non autorisé." });

        var tailleMaxOctets = _configuration.GetValue<long?>("Gemini:VideoTailleMaxOctets") ?? 200_000_000;
        if (video.Length > tailleMaxOctets)
            return BadRequest(new { erreur = $"La vidéo dépasse la taille maximale autorisée ({tailleMaxOctets / 1_000_000} Mo)." });

        await using var flux = video.OpenReadStream();
        var commandeVideo = new ExtraireVideoCommand
        {
            Flux = flux,
            TypeMime = video.ContentType,
            TailleOctets = video.Length
        };

        var recette = await _mediateur.Send(commandeVideo, annulation);
        return Ok(recette);
    }
}

public record RequeteExtractionIngredients(string? ImageBase64, string? TypeMime);
