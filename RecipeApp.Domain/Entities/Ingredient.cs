using RecipeApp.Domain.Enums;

namespace RecipeApp.Domain.Entities;

/// <summary>Ingrédient appartenant à une recette.</summary>
public class Ingredient
{
    public Guid Id { get; set; }
    public Guid RecetteId { get; set; }
    public string Nom { get; set; } = string.Empty;
    public decimal Quantite { get; set; }
    public UniteIngredient Unite { get; set; }

    /// <summary>Position dans la liste des ingrédients (pour le tri).</summary>
    public int Ordre { get; set; }

    // Propriété de navigation
    public Recette Recette { get; set; } = null!;
}