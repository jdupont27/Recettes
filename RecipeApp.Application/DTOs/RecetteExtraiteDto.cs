namespace RecipeApp.Application.DTOs;

public class RecetteExtraiteDto
{
    public string Titre { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int TempsPreparation { get; set; }
    public int TempsCuisson { get; set; }
    public int NombrePortions { get; set; } = 4;
    public string? Difficulte { get; set; }
    public string? TypeCuisine { get; set; }
    public List<IngredientDto> Ingredients { get; set; } = new();
    public List<EtapeDto> Etapes { get; set; } = new();
}
