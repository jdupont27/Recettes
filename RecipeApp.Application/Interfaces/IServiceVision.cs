using RecipeApp.Application.DTOs;

namespace RecipeApp.Application.Interfaces;

public interface IServiceVision
{
    Task<RecetteExtraiteDto> ExtraireRecetteAsync(string imageBase64, string typeMime, CancellationToken annulation = default);
}
