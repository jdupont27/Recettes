using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RecipeApp.Domain.Enums;
using RecipeApp.Infrastructure.Identity;

namespace RecipeApp.Web.Controllers;

/// <summary>Actions de compte qui doivent écrire le cookie d'authentification (impossible depuis un
/// composant Blazor Server interactif, dont le cycle de vie ne correspond plus à une requête HTTP unique).</summary>
[Route("compte")]
[Authorize]
public class CompteController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IAntiforgery _antiforgery;

    public CompteController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IAntiforgery antiforgery)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _antiforgery = antiforgery;
    }

    [HttpPost("theme/enregistrer")]
    public async Task<IActionResult> EnregistrerTheme([FromForm] int theme)
    {
        await _antiforgery.ValidateRequestAsync(HttpContext);

        if (Enum.IsDefined(typeof(ThemeVisuel), theme))
        {
            var utilisateur = await _userManager.GetUserAsync(User);
            if (utilisateur != null)
            {
                utilisateur.ThemeVisuel = (ThemeVisuel)theme;
                await _userManager.UpdateAsync(utilisateur);
                await _signInManager.RefreshSignInAsync(utilisateur);
            }
        }

        return Redirect("/compte/theme");
    }
}
