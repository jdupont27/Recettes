using RecipeApp.Domain.Enums;

namespace RecipeApp.Domain.Entities;

/// <summary>Entité principale représentant une recette de cuisine.</summary>
public class Recette
{
    public Guid Id { get; set; }
    public string Titre { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>Temps de préparation en minutes.</summary>
    public int TempsPreparation { get; set; }

    /// <summary>Temps de cuisson en minutes.</summary>
    public int TempsCuisson { get; set; }

    public int NombrePortions { get; set; }
    public Difficulte Difficulte { get; set; }
    public string? TypeCuisine { get; set; }
    public Visibilite Visibilite { get; set; }
    public DateTime DateCreation { get; set; }
    public DateTime? DateModification { get; set; }
    public Guid AuteurId { get; set; }
    public string? CheminImage { get; set; }

    // Propriétés de navigation
    public ICollection<Ingredient> Ingredients { get; set; } = new List<Ingredient>();
    public ICollection<EtapeRecette> Etapes { get; set; } = new List<EtapeRecette>();
    public ICollection<PartageRecette> Partages { get; set; } = new List<PartageRecette>();
    public ICollection<RecetteCategorie> RecetteCategories { get; set; } = new List<RecetteCategorie>();
    public ICollection<RecetteEtiquette> RecetteEtiquettes { get; set; } = new List<RecetteEtiquette>();
}