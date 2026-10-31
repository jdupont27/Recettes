namespace RecipeApp.Application.Interfaces;

public interface IServiceCourriel
{
    Task EnvoyerAsync(string destinataire, string sujet, string corpsHtml, CancellationToken annulation = default);
}
