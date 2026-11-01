using MediatR;
using RecipeApp.Application.DTOs;

namespace RecipeApp.Application.Vision.Commands.ExtraireIngredients;

public record ExtraireIngredientsCommand : IRequest<RecetteExtraiteDto>
{
    public List<ImageAnalyseeDto> Images { get; init; } = new();
}
