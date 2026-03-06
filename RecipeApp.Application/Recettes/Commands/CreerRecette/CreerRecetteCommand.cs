using MediatR;
using RecipeApp.Application.DTOs;
using RecipeApp.Domain.Enums;

namespace RecipeApp.Application.Recettes.Commands.CreerRecette;

public record CreerRecetteCommand : IRequest<RecetteDto>
{
    public string Titre { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int TempsPreparation { get; init; }
    public int TempsCuisson { get; init; }
    public int NombrePortions { get; init; }
    public Difficulte Difficulte { get; init; }
    public string? TypeCuisine { get; init; }
    public Visibilite Visibilite { get; init; }
    public Guid AuteurId { get; init; }
    public string? ImageBase64 { get; init; }
    public string? ImageTypeMime { get; init; }
    public List<IngredientDto> Ingredients { get; init; } = new();
    public List<EtapeDto> Etapes { get; init; } = new();
    public List<Guid> CategoriesIds { get; init; } = new();
    public List<Guid> EtiquettesIds { get; init; } = new();
}