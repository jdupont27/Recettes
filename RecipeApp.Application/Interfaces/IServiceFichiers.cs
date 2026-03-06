namespace RecipeApp.Application.Interfaces;

public interface IServiceFichiers
{
    Task<string> SauvegarderImageAsync(string imageBase64, string typeMime, CancellationToken annulation = default);
    Task SupprimerImageAsync(string cheminImage, CancellationToken annulation = default);
}