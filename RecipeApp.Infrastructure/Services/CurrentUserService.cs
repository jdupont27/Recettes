using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using RecipeApp.Application.Interfaces;

namespace RecipeApp.Infrastructure.Services;

/// <summary>Récupère les informations de l'utilisateur connecté depuis le HttpContext.</summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UtilisateurId
    {
        get
        {
            var valeur = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(valeur, out var guid) ? guid : null;
        }
    }

    public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);

    public bool EstAuthentifie => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
