using RecipeApp.Domain.Enums;

namespace RecipeApp.Domain.Interfaces;

/// <summary>Critères de recherche pour filtrer les recettes.</summary>
public class FiltreRecette
{
    public string? TexteLibre { get; set; }
    public int? TempsMaxMinutes { get; set; }
    public int? NombrePortions { get; set; }
    public Guid? CategorieId { get; set; }
    public string? TypeCuisine { get; set; }
    public Difficulte? Difficulte { get; set; }
    public List<Guid> EtiquettesIds { get; set; } = new();
    public List<string> IngredientsDisponibles { get; set; } = new();
    public Guid? AuteurId { get; set; }
    public bool IncluirePubliques { get; set; } = true;
    public bool IncluirePartagees { get; set; } = false;
    public Guid? UtilisateurConnecteId { get; set; }
    public int Page { get; set; } = 1;
    public int TaillePage { get; set; } = 12;
}