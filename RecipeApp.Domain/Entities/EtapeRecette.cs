namespace RecipeApp.Domain.Entities;

/// <summary>Étape de préparation d'une recette.</summary>
public class EtapeRecette
{
    public Guid Id { get; set; }
    public Guid RecetteId { get; set; }
    public int NumeroEtape { get; set; }
    public string Description { get; set; } = string.Empty;

    // Propriété de navigation
    public Recette Recette { get; set; } = null!;
}