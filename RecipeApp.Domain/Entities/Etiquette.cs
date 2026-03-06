namespace RecipeApp.Domain.Entities;

/// <summary>Étiquette/tag sur une recette (ex. : Végétarien, Sans gluten).</summary>
public class Etiquette
{
    public Guid Id { get; set; }
    public string Nom { get; set; } = string.Empty;

    // Propriétés de navigation
    public ICollection<RecetteEtiquette> RecetteEtiquettes { get; set; } = new List<RecetteEtiquette>();
}