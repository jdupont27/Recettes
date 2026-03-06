namespace RecipeApp.Domain.Entities;

/// <summary>Catégorie de recette (ex. : Entrée, Plat principal, Dessert).</summary>
public class Categorie
{
    public Guid Id { get; set; }
    public string Nom { get; set; } = string.Empty;

    // Propriétés de navigation
    public ICollection<RecetteCategorie> RecetteCategories { get; set; } = new List<RecetteCategorie>();
}