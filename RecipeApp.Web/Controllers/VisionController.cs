using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecipeApp.Application.DTOs;
using RecipeApp.Application.Vision.Commands.ExtraireIngredients;

namespace RecipeApp.Web.Controllers;

/// <summary>API d'extraction d'ingrédients par photo via Anthropic Vision.</summary>
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
    /// Extrait les ingrédients depuis une image en base64.
    /// POST /api/vision/extraire-ingredients
    /// Corps : { "imageBase64": "...", "typeMime": "image/jpeg" }
    /// </summary>
    [HttpPost("extraire-ingredients")]
    public async Task<ActionResult<List<IngredientDto>>> ExtraireIngredients(
        [FromBody] RequeteExtractionIngredients requete,
        CancellationToken annulation)
    {
        if (string.IsNullOrEmpty(requete.ImageBase64))
            return BadRequest(new { erreur = "L'image est obligatoire." });

        var commande = new ExtraireIngredientsCommand
        {
            ImageBase64 = requete.ImageBase64,
            TypeMime = requete.TypeMime ?? "image/jpeg"
        };

        var ingredients = await _mediateur.Send(commande, annulation);
        return Ok(ingredients);
    }
}

public record RequeteExtractionIngredients(string? ImageBase64, string? TypeMime);
