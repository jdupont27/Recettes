namespace RecipeApp.Domain.Entities;

/// <summary>Table de jointure entre Recette et Etiquette.</summary>
public class RecetteEtiquette
{
    public Guid RecetteId { get; set; }
    public Guid EtiquetteId { get; set; }

    // Propriétés de navigation
    public Recette Recette { get; set; } = null!;
    public Etiquette Etiquette { get; set; } = null!;
}