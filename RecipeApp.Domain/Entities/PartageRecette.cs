namespace RecipeApp.Domain.Entities;

/// <summary>Représente le partage d'une recette privée avec un utilisateur invité.</summary>
public class PartageRecette
{
    public Guid RecetteId { get; set; }
    public Guid UtilisateurId { get; set; }
    public DateTime DatePartage { get; set; }

    // Propriétés de navigation
    public Recette Recette { get; set; } = null!;
}