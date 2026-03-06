using MediatR;
using RecipeApp.Application.DTOs;
using RecipeApp.Domain.Enums;

namespace RecipeApp.Application.Recettes.Queries.RechercherRecettes;

public record RechercherRecettesQuery : IRequest<ResultatPagine<RecetteDto>>
{
    public string? TexteLibre { get; init; }
    public int? TempsMaxMinutes { get; init; }
    public int? NombrePortions { get; init; }
    public Guid? CategorieId { get; init; }
    public string? TypeCuisine { get; init; }
    public Difficulte? Difficulte { get; init; }
    public List<Guid> EtiquettesIds { get; init; } = new();
    public List<string> IngredientsDisponibles { get; init; } = new();
    public Guid? AuteurId { get; init; }
    public bool IncluirePubliques { get; init; } = true;
    public bool IncluirePartagees { get; init; } = false;
    public Guid? UtilisateurConnecteId { get; init; }
    public int Page { get; init; } = 1;
    public int TaillePage { get; init; } = 12;
}
