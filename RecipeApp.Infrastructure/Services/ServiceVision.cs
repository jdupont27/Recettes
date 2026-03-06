using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RecipeApp.Application.DTOs;
using RecipeApp.Application.Interfaces;
using RecipeApp.Domain.Enums;

namespace RecipeApp.Infrastructure.Services;

/// <summary>Service qui utilise l'API Anthropic Vision pour extraire des ingrédients depuis une photo.</summary>
public class ServiceVision : IServiceVision
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _modele;
    private readonly ILogger<ServiceVision> _logger;

    public ServiceVision(HttpClient httpClient, IConfiguration configuration, ILogger<ServiceVision> logger)
    {
        _httpClient = httpClient;
        _apiKey = configuration["Anthropic:ApiKey"] ?? throw new InvalidOperationException("Clé API Anthropic manquante.");
        _modele = configuration["Anthropic:Model"] ?? "claude-opus-4-6";
        _logger = logger;
    }

    public async Task<List<IngredientDto>> ExtraireIngredientsAsync(string imageBase64, string typeMime, CancellationToken annulation = default)
    {
        var corps = new
        {
            model = _modele,
            max_tokens = 1024,
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new
                        {
                            type = "image",
                            source = new
                            {
                                type = "base64",
                                media_type = typeMime,
                                data = imageBase64
                            }
                        },
                        new
                        {
                            type = "text",
                            text = "Analyse cette image et extrait la liste d'ingrédients visible. Retourne UNIQUEMENT un JSON valide sans texte autour, avec ce format exact : {\"ingredients\": [{\"nom\": \"nom de l'ingrédient\", \"quantite\": \"0\", \"unite\": \"Unite\"}]}. Si une quantité ou unité est visible, utilise-la. Les unités possibles sont : Tasse, Grammes, Kilogrammes, Millilitres, Litres, CuillereSoupe, CuillereThe, Unite, AuGout."
                        }
                    }
                }
            }
        };

        var json = JsonSerializer.Serialize(corps);
        using var requete = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages");
        requete.Headers.Add("x-api-key", _apiKey);
        requete.Headers.Add("anthropic-version", "2023-06-01");
        requete.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var reponse = await _httpClient.SendAsync(requete, annulation);
        reponse.EnsureSuccessStatusCode();

        var contenu = await reponse.Content.ReadAsStringAsync(annulation);
        var reponseApi = JsonSerializer.Deserialize<ReponseAnthropic>(contenu, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        var texteReponse = reponseApi?.Content?.FirstOrDefault()?.Text ?? "{}";
        _logger.LogDebug("Réponse Vision API : {Reponse}", texteReponse);

        try
        {
            // Nettoyer le texte si entouré de backticks markdown
            var texteNettoye = texteReponse.Trim();
            if (texteNettoye.StartsWith("```"))
                texteNettoye = string.Join('\n', texteNettoye.Split('\n').Skip(1).SkipLast(1));

            var resultat = JsonSerializer.Deserialize<ResultatExtraction>(texteNettoye, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return resultat?.Ingredients?.Select(i => new IngredientDto
            {
                Id = Guid.NewGuid(),
                Nom = i.Nom ?? string.Empty,
                Quantite = decimal.TryParse(i.Quantite, out var q) ? q : 0,
                Unite = ParseUnite(i.Unite),
                Ordre = 0
            }).ToList() ?? new List<IngredientDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'analyse de la réponse Vision : {Reponse}", texteReponse);
            return new List<IngredientDto>();
        }
    }

    private static UniteIngredient ParseUnite(string? unite) => unite?.ToLower() switch
    {
        "tasse" => UniteIngredient.Tasse,
        "grammes" or "g" or "gr" => UniteIngredient.Grammes,
        "kilogrammes" or "kg" => UniteIngredient.Kilogrammes,
        "millilitres" or "ml" => UniteIngredient.Millilitres,
        "litres" or "l" => UniteIngredient.Litres,
        "cuilleresoup" or "cuilleresoupe" or "c. à soupe" or "tbsp" => UniteIngredient.CuillereSoupe,
        "cuillerethe" or "cuillereté" or "c. à thé" or "tsp" => UniteIngredient.CuillereThe,
        "augout" or "au goût" => UniteIngredient.AuGout,
        _ => UniteIngredient.Unite
    };

    // Classes internes pour désérialiser la réponse Anthropic
    private class ReponseAnthropic
    {
        public List<ContenuAnthropic>? Content { get; set; }
    }

    private class ContenuAnthropic
    {
        public string? Text { get; set; }
    }

    private class ResultatExtraction
    {
        public List<IngredientExtrait>? Ingredients { get; set; }
    }

    private class IngredientExtrait
    {
        public string? Nom { get; set; }
        public string? Quantite { get; set; }
        public string? Unite { get; set; }
    }
}
