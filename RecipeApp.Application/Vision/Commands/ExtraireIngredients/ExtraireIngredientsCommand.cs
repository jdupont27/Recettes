using MediatR;
using RecipeApp.Application.DTOs;

namespace RecipeApp.Application.Vision.Commands.ExtraireIngredients;

public record ExtraireIngredientsCommand : IRequest<RecetteExtraiteDto>
{
    public string ImageBase64 { get; init; } = string.Empty;
    public string TypeMime { get; init; } = "image/jpeg";
}
