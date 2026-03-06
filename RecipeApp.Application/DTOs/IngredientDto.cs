using RecipeApp.Domain.Enums;

namespace RecipeApp.Application.DTOs;

public class IngredientDto
{
    public Guid Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public decimal Quantite { get; set; }
    public UniteIngredient Unite { get; set; }
    public int Ordre { get; set; }
}