using RecipeApp.Domain.Enums;

namespace RecipeApp.Application.DTOs;

public class RecetteDto
{
    public Guid Id { get; set; }
    public string Titre { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int TempsPreparation { get; set; }
    public int TempsCuisson { get; set; }
    public int NombrePortions { get; set; }
    public Difficulte Difficulte { get; set; }
    public string? TypeCuisine { get; set; }
    public Visibilite Visibilite { get; set; }
    public DateTime DateCreation { get; set; }
    public Guid AuteurId { get; set; }
    public string? NomAuteur { get; set; }
    public string? CheminImage { get; set; }
    public List<IngredientDto> Ingredients { get; set; } = new();
    public List<EtapeDto> Etapes { get; set; } = new();
    public List<CategorieDto> Categories { get; set; } = new();
    public List<EtiquetteDto> Etiquettes { get; set; } = new();
}