namespace RecipeApp.Application.Interfaces;

public interface IServiceUtilisateurs
{
    Task<string?> ObtenirNomAsync(Guid utilisateurId, CancellationToken annulation = default);
}
