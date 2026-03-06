using MediatR;
using RecipeApp.Application.DTOs;

namespace RecipeApp.Application.Vision.Commands.ExtraireIngredients;

public record ExtraireIngredientsCommand : IRequest<List<IngredientDto>>
{
    public string ImageBase64 { get; init; } = string.Empty;
    public string TypeMime { get; init; } = "image/jpeg";
}