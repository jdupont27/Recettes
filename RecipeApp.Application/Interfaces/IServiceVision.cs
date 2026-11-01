using RecipeApp.Application.DTOs;

namespace RecipeApp.Application.Interfaces;

public interface IServiceVision
{
    Task<RecetteExtraiteDto> ExtraireRecetteAsync(IReadOnlyList<ImageAnalyseeDto> images, CancellationToken annulation = default);
}
