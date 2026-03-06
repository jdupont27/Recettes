using FluentValidation;

namespace RecipeApp.Web.Middleware;

/// <summary>Middleware global de gestion des erreurs non interceptées.</summary>
public class GestionErreurs
{
    private readonly RequestDelegate _suivant;
    private readonly ILogger<GestionErreurs> _logger;

    public GestionErreurs(RequestDelegate suivant, ILogger<GestionErreurs> logger)
    {
        _suivant = suivant;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext contexte)
    {
        try
        {
            await _suivant(contexte);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur non gérée : {Message}", ex.Message);
            await GererErreurAsync(contexte, ex);
        }
    }

    private static async Task GererErreurAsync(HttpContext contexte, Exception exception)
    {
        // Ne pas réécrire si la réponse a déjà commencé
        if (contexte.Response.HasStarted)
            return;

        contexte.Response.ContentType = "application/json";
        contexte.Response.StatusCode = exception switch
        {
            ValidationException => StatusCodes.Status400BadRequest,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };

        var reponse = new
        {
            erreur = exception.Message,
            code = contexte.Response.StatusCode
        };

        await contexte.Response.WriteAsJsonAsync(reponse);
    }
}
