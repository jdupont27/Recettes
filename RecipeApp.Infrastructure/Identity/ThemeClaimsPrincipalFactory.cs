using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace RecipeApp.Infrastructure.Identity;

/// <summary>Ajoute le thème visuel de l'utilisateur aux claims du cookie d'authentification,
/// pour que MainLayout puisse le lire sans requête DbContext supplémentaire (évite les
/// accès concurrents au contexte scoped partagé par UserManager/SignInManager).</summary>
public class ThemeClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole<Guid>>
{
    public const string TypeClaim = "ThemeVisuel";

    public ThemeClaimsPrincipalFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        IOptions<IdentityOptions> optionsAccessor)
        : base(userManager, roleManager, optionsAccessor)
    {
    }

    public override async Task<ClaimsPrincipal> CreateAsync(ApplicationUser user)
    {
        var principal = await base.CreateAsync(user);
        ((ClaimsIdentity)principal.Identity!).AddClaim(new Claim(TypeClaim, ((int)user.ThemeVisuel).ToString()));
        return principal;
    }
}
