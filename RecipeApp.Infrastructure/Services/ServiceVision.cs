using System.Globalization;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RecipeApp.Application.DTOs;
using RecipeApp.Application.Interfaces;
using RecipeApp.Domain.Enums;

namespace RecipeApp.Infrastructure.Services;

/// <summary>Service qui utilise l'API Gemini Vision pour extraire une recette complète depuis une photo.</summary>
public class ServiceVision : IServiceVision
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _modele;
    private readonly ILogger<ServiceVision> _logger;

    public ServiceVision(HttpClient httpClient, IConfiguration configuration, ILogger<ServiceVision> logger)
    {
        _httpClient = httpClient;
        _apiKey = configuration["Gemini:ApiKey"] ?? throw new InvalidOperationException("Clé API Gemini manquante (Gemini:ApiKey).");
        _modele = configuration["Gemini:Model"] ?? "gemini-2.5-flash";
        _logger = logger;
    }

    public async Task<RecetteExtraiteDto> ExtraireRecetteAsync(string imageBase64, string typeMime, CancellationToken annulation = default)
    {
        const string prompt = """
            Tu es un assistant culinaire. En observant cette image, crée une fiche recette structurée en JSON.
            Ne reproduis pas mot pour mot un texte existant — formule les étapes avec tes propres mots.
            Retourne UNIQUEMENT un JSON valide (sans balises markdown) avec ce format :
            {
              "titre": "nom de la recette",
              "description": "une courte description originale",
              "tempsPreparation": 30,
              "tempsCuisson": 20,
              "nombrePortions": 4,
              "difficulte": "Facile",
              "typeCuisine": "Française",
              "ingredients": [{"nom": "farine", "quantite": "250", "unite": "Grammes"}],
              "etapes": [{"numeroEtape": 1, "description": "Décrire l'étape avec tes propres mots..."}]
            }
            Valeurs possibles pour difficulte : Facile, Moyen, Difficile.
            Valeurs possibles pour unite : Tasse, Grammes, Kilogrammes, Millilitres, Litres, CuillereSoupe, CuillereThe, Unite, AuGout.
            Si une information n'est pas visible, utilise null pour les chaînes ou une valeur par défaut pour les entiers (nombrePortions=4, temps=0).
            """;

        var corps = new
        {
            contents = new[]
            {
                new
                {
                    parts = new object[]
                    {
                        new
                        {
                            inlineData = new { mimeType = typeMime, data = imageBase64 }
                        },
                        new { text = prompt }
                    }
                }
            }
        };

        var json = JsonSerializer.Serialize(corps);
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_modele}:generateContent?key={_apiKey}";

        using var requete = new HttpRequestMessage(HttpMethod.Post, url);
        requete.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var reponse = await _httpClient.SendAsync(requete, annulation);
        reponse.EnsureSuccessStatusCode();

        var contenu = await reponse.Content.ReadAsStringAsync(annulation);
        _logger.LogInformation("Réponse brute Gemini : {Reponse}", contenu);

        try
        {
            var reponseApi = JsonSerializer.Deserialize<ReponseGemini>(contenu, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var candidat = reponseApi?.Candidates?.FirstOrDefault();
            if (candidat?.Content == null)
            {
                _logger.LogWarning("Gemini n'a pas généré de contenu. finishReason={Raison}", candidat?.FinishReason ?? "inconnu");
                return new RecetteExtraiteDto();
            }

            var texteJson = candidat.Content.Parts?.FirstOrDefault()?.Text ?? "{}";
            _logger.LogInformation("JSON extrait Gemini : {Json}", texteJson);

            // Nettoyer si entouré de backticks markdown
            var texteNettoye = texteJson.Trim();
            if (texteNettoye.StartsWith("```"))
                texteNettoye = string.Join('\n', texteNettoye.Split('\n').Skip(1).SkipLast(1));

            var raw = JsonSerializer.Deserialize<RecetteExtraiteRaw>(texteNettoye, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new RecetteExtraiteRaw();

            return new RecetteExtraiteDto
            {
                Titre = raw.Titre ?? string.Empty,
                Description = raw.Description,
                TempsPreparation = raw.TempsPreparation,
                TempsCuisson = raw.TempsCuisson,
                NombrePortions = raw.NombrePortions > 0 ? raw.NombrePortions : 4,
                Difficulte = raw.Difficulte,
                TypeCuisine = raw.TypeCuisine,
                Ingredients = raw.Ingredients?.Select((i, idx) => new IngredientDto
                {
                    Id = Guid.NewGuid(),
                    Nom = i.Nom ?? string.Empty,
                    Quantite = decimal.TryParse(i.Quantite, NumberStyles.Any, CultureInfo.InvariantCulture, out var q) ? q : 0,
                    Unite = ParseUnite(i.Unite),
                    Ordre = idx + 1
                }).ToList() ?? new List<IngredientDto>(),
                Etapes = raw.Etapes?.Select((e, idx) => new EtapeDto
                {
                    Id = Guid.NewGuid(),
                    NumeroEtape = e.NumeroEtape > 0 ? e.NumeroEtape : idx + 1,
                    Description = e.Description ?? string.Empty
                }).ToList() ?? new List<EtapeDto>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'analyse de la réponse Gemini : {Reponse}", contenu);
            return new RecetteExtraiteDto();
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

    // Désérialisation de la réponse Gemini
    private class ReponseGemini { public List<CandidatGemini>? Candidates { get; set; } }
    private class CandidatGemini { public ContenuGemini? Content { get; set; } public string? FinishReason { get; set; } }
    private class ContenuGemini { public List<PartieGemini>? Parts { get; set; } }
    private class PartieGemini { public string? Text { get; set; } }

    // Désérialisation du JSON de recette extrait
    private class RecetteExtraiteRaw
    {
        public string? Titre { get; set; }
        public string? Description { get; set; }
        public int TempsPreparation { get; set; }
        public int TempsCuisson { get; set; }
        public int NombrePortions { get; set; }
        public string? Difficulte { get; set; }
        public string? TypeCuisine { get; set; }
        public List<IngredientRaw>? Ingredients { get; set; }
        public List<EtapeRaw>? Etapes { get; set; }
    }

    private class IngredientRaw { public string? Nom { get; set; } public string? Quantite { get; set; } public string? Unite { get; set; } }
    private class EtapeRaw { public int NumeroEtape { get; set; } public string? Description { get; set; } }
}
