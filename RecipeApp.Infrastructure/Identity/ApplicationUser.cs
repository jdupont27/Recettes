using Microsoft.AspNetCore.Identity;
using RecipeApp.Domain.Enums;

namespace RecipeApp.Infrastructure.Identity;

/// <summary>Utilisateur de l'application étendu avec le nom d'affichage.</summary>
public class ApplicationUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = string.Empty;
    public DateTime DateInscription { get; set; } = DateTime.UtcNow;
    public ThemeVisuel ThemeVisuel { get; set; } = ThemeVisuel.Classique;
}
