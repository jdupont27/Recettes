namespace RecipeApp.Domain.Entities;

/// <summary>Table de jointure entre Recette et Categorie.</summary>
public class RecetteCategorie
{
    public Guid RecetteId { get; set; }
    public Guid CategorieId { get; set; }

    // Propriétés de navigation
    public Recette Recette { get; set; } = null!;
    public Categorie Categorie { get; set; } = null!;
}