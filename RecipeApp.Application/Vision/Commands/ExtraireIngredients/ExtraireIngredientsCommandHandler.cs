using MediatR;
using RecipeApp.Application.DTOs;
using RecipeApp.Application.Interfaces;

namespace RecipeApp.Application.Vision.Commands.ExtraireIngredients;

public class ExtraireIngredientsCommandHandler : IRequestHandler<ExtraireIngredientsCommand, List<IngredientDto>>
{
    private readonly IServiceVision _serviceVision;

    public ExtraireIngredientsCommandHandler(IServiceVision serviceVision)
    {
        _serviceVision = serviceVision;
    }

    public async Task<List<IngredientDto>> Handle(ExtraireIngredientsCommand commande, CancellationToken annulation)
    {
        return await _serviceVision.ExtraireIngredientsAsync(commande.ImageBase64, commande.TypeMime, annulation);
    }
}