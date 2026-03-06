namespace RecipeApp.Domain.Interfaces;

public interface IUnitOfWork
{
    Task<int> SauvegarderAsync(CancellationToken annulation = default);
}