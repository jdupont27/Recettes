using MediatR;
using RecipeApp.Application.DTOs;
using RecipeApp.Application.Interfaces;

namespace RecipeApp.Application.Vision.Commands.ExtraireIngredients;

public class ExtraireIngredientsCommandHandler : IRequestHandler<ExtraireIngredientsCommand, RecetteExtraiteDto>
{
    private readonly IServiceVision _serviceVision;

    public ExtraireIngredientsCommandHandler(IServiceVision serviceVision)
    {
        _serviceVision = serviceVision;
    }

    public async Task<RecetteExtraiteDto> Handle(ExtraireIngredientsCommand commande, CancellationToken annulation)
    {
        return await _serviceVision.ExtraireRecetteAsync(commande.ImageBase64, commande.TypeMime, annulation);
    }
}
