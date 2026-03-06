namespace RecipeApp.Application.Interfaces;

public interface ICurrentUserService
{
    Guid? UtilisateurId { get; }
    string? Email { get; }
    bool EstAuthentifie { get; }
}