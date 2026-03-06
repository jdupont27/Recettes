using RecipeApp.Application.DTOs;

namespace RecipeApp.Application.Interfaces;

public interface IServiceVision
{
    Task<List<IngredientDto>> ExtraireIngredientsAsync(string imageBase64, string typeMime, CancellationToken annulation = default);
}