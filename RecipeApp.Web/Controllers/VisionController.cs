using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RecipeApp.Application.DTOs;
using RecipeApp.Application.Vision.Commands.ExtraireIngredients;

namespace RecipeApp.Web.Controllers;

/// <summary>API d'extraction de recette par photo via Gemini Vision.</summary>
[ApiController]
[Route("api/vision")]
[Authorize]
public class VisionController : ControllerBase
{
    private readonly IMediator _mediateur;

    public VisionController(IMediator mediateur)
    {
        _mediateur = mediateur;
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
}

public record RequeteExtractionIngredients(string? ImageBase64, string? TypeMime);
