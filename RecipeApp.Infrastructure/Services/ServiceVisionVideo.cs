using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RecipeApp.Application.DTOs;
using RecipeApp.Application.Exceptions;
using RecipeApp.Application.Interfaces;
using RecipeApp.Domain.Enums;

namespace RecipeApp.Infrastructure.Services;

/// <summary>Service qui utilise la File API de Gemini pour extraire une recette complète depuis une vidéo de cuisine.</summary>
public class ServiceVisionVideo : IServiceVisionVideo
{
    private static readonly JsonSerializerOptions OptionsJson = new() { PropertyNameCaseInsensitive = true };
    private static readonly TimeSpan DelaiMaxTraitement = TimeSpan.FromMinutes(2);

    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _modele;
    private readonly ILogger<ServiceVisionVideo> _logger;

    public ServiceVisionVideo(HttpClient httpClient, IConfiguration configuration, ILogger<ServiceVisionVideo> logger)
    {
        _httpClient = httpClient;
        _apiKey = configuration["Gemini:ApiKey"] ?? throw new InvalidOperationException("Clé API Gemini manquante (Gemini:ApiKey).");
        _modele = configuration["Gemini:VideoModel"] ?? "gemini-3.5-flash-lite";
        _logger = logger;
    }

    public async Task<RecetteExtraiteDto> ExtraireRecetteDeFichierAsync(Stream flux, string typeMime, long tailleOctets, CancellationToken annulation = default)
    {
        var fichier = await UploaderFichierAsync(flux, typeMime, tailleOctets, annulation);
        try
        {
            fichier = await AttendreFichierActifAsync(fichier.Name!, annulation);
            var partieMedia = new { fileData = new { mimeType = typeMime, fileUri = fichier.Uri } };
            var texteJson = await AppellerGenerateContentAsync(partieMedia, annulation);
            return ConvertirEnRecette(texteJson);
        }
        finally
        {
            await SupprimerFichierAsync(fichier.Name);
        }
    }

    public async Task<RecetteExtraiteDto> ExtraireRecetteDeUrlYoutubeAsync(string urlYoutube, CancellationToken annulation = default)
    {
        var partieMedia = new { fileData = new { fileUri = urlYoutube } };
        var texteJson = await AppellerGenerateContentAsync(partieMedia, annulation);
        return ConvertirEnRecette(texteJson);
    }

    private async Task<FichierGemini> UploaderFichierAsync(Stream flux, string typeMime, long tailleOctets, CancellationToken annulation)
    {
        var urlDemarrage = $"https://generativelanguage.googleapis.com/upload/v1beta/files?key={_apiKey}";
        using var requeteDemarrage = new HttpRequestMessage(HttpMethod.Post, urlDemarrage)
        {
            Content = new StringContent("""{"file":{"display_name":"video-recette"}}""", Encoding.UTF8, "application/json")
        };
        requeteDemarrage.Headers.Add("X-Goog-Upload-Protocol", "resumable");
        requeteDemarrage.Headers.Add("X-Goog-Upload-Command", "start");
        requeteDemarrage.Headers.Add("X-Goog-Upload-Header-Content-Length", tailleOctets.ToString(CultureInfo.InvariantCulture));
        requeteDemarrage.Headers.Add("X-Goog-Upload-Header-Content-Type", typeMime);

        var reponseDemarrage = await _httpClient.SendAsync(requeteDemarrage, annulation);
        await VerifierReponseAsync(reponseDemarrage, "L'envoi de la vidéo à Gemini a échoué (initialisation).", annulation);

        var urlUpload = reponseDemarrage.Headers.TryGetValues("X-Goog-Upload-URL", out var valeurs) ? valeurs.FirstOrDefault() : null;
        if (string.IsNullOrEmpty(urlUpload))
            throw new ExtractionVideoException("Gemini n'a pas fourni d'URL d'envoi pour la vidéo.");

        using var requeteUpload = new HttpRequestMessage(HttpMethod.Post, urlUpload)
        {
            Content = new StreamContent(flux)
        };
        requeteUpload.Content.Headers.ContentLength = tailleOctets;
        requeteUpload.Headers.Add("X-Goog-Upload-Command", "upload, finalize");
        requeteUpload.Headers.Add("X-Goog-Upload-Offset", "0");

        var reponseUpload = await _httpClient.SendAsync(requeteUpload, annulation);
        await VerifierReponseAsync(reponseUpload, "L'envoi de la vidéo à Gemini a échoué.", annulation);

        var contenuUpload = await reponseUpload.Content.ReadAsStringAsync(annulation);
        var enveloppe = JsonSerializer.Deserialize<FichierGeminiEnveloppe>(contenuUpload, OptionsJson);
        return enveloppe?.File ?? throw new ExtractionVideoException("Réponse invalide de Gemini lors de l'envoi de la vidéo.");
    }

    private async Task<FichierGemini> AttendreFichierActifAsync(string nomFichier, CancellationToken annulation)
    {
        var chrono = Stopwatch.StartNew();
        while (true)
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/{nomFichier}?key={_apiKey}";
            var reponse = await _httpClient.GetAsync(url, annulation);
            await VerifierReponseAsync(reponse, "Impossible de vérifier l'état de la vidéo sur Gemini.", annulation);

            var contenu = await reponse.Content.ReadAsStringAsync(annulation);
            var fichier = JsonSerializer.Deserialize<FichierGemini>(contenu, OptionsJson)
                ?? throw new ExtractionVideoException("Réponse invalide de Gemini lors du suivi de la vidéo.");

            if (fichier.State == "ACTIVE")
                return fichier;
            if (fichier.State == "FAILED")
                throw new ExtractionVideoException("Le traitement de la vidéo par Gemini a échoué.");

            if (chrono.Elapsed > DelaiMaxTraitement)
                throw new ExtractionVideoException("Le traitement de la vidéo par Gemini prend trop de temps.", 504);

            await Task.Delay(TimeSpan.FromSeconds(3), annulation);
        }
    }

    private async Task SupprimerFichierAsync(string? nomFichier)
    {
        if (string.IsNullOrEmpty(nomFichier))
            return;

        try
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/{nomFichier}?key={_apiKey}";
            await _httpClient.DeleteAsync(url, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Impossible de supprimer le fichier vidéo temporaire {Fichier} sur Gemini.", nomFichier);
        }
    }

    private async Task<string> AppellerGenerateContentAsync(object partieMedia, CancellationToken annulation)
    {
        var corps = new
        {
            contents = new[]
            {
                new { parts = new object[] { partieMedia, new { text = ConstruirePrompt() } } }
            },
            generationConfig = new { mediaResolution = "MEDIA_RESOLUTION_LOW" }
        };

        var json = JsonSerializer.Serialize(corps);
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_modele}:generateContent?key={_apiKey}";

        using var requete = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        var reponse = await _httpClient.SendAsync(requete, annulation);
        await VerifierReponseAsync(reponse, "L'analyse de la vidéo par Gemini a échoué.", annulation);

        var contenu = await reponse.Content.ReadAsStringAsync(annulation);
        _logger.LogInformation("Réponse brute Gemini (vidéo) : {Reponse}", contenu);

        var reponseApi = JsonSerializer.Deserialize<ReponseGemini>(contenu, OptionsJson);
        var candidat = reponseApi?.Candidates?.FirstOrDefault();
        var texte = candidat?.Content?.Parts?.FirstOrDefault()?.Text;

        if (string.IsNullOrEmpty(texte))
        {
            _logger.LogWarning("Gemini n'a pas généré de contenu pour la vidéo. finishReason={Raison}", candidat?.FinishReason ?? "inconnu");
            throw new ExtractionVideoException("Gemini n'a pas pu analyser cette vidéo.");
        }

        return texte;
    }

    private async Task VerifierReponseAsync(HttpResponseMessage reponse, string messageEchec, CancellationToken annulation)
    {
        if (reponse.IsSuccessStatusCode)
            return;

        if (reponse.StatusCode == HttpStatusCode.TooManyRequests)
            throw new ExtractionVideoException("Quota Gemini dépassé, réessayez plus tard.", 429);

        var corps = await reponse.Content.ReadAsStringAsync(annulation);
        _logger.LogError("Erreur Gemini ({Code}) : {Corps}", (int)reponse.StatusCode, corps);
        throw new ExtractionVideoException(messageEchec);
    }

    private RecetteExtraiteDto ConvertirEnRecette(string texteJson)
    {
        var texteNettoye = texteJson.Trim();
        if (texteNettoye.StartsWith("```"))
            texteNettoye = string.Join('\n', texteNettoye.Split('\n').Skip(1).SkipLast(1));

        RecetteExtraiteRaw raw;
        try
        {
            raw = JsonSerializer.Deserialize<RecetteExtraiteRaw>(texteNettoye, OptionsJson)
                ?? throw new JsonException("Réponse vide.");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON invalide retourné par Gemini pour la vidéo : {Reponse}", texteJson);
            throw new ExtractionVideoException("Gemini a retourné une réponse invalide pour cette vidéo.");
        }

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
                Ordre = idx + 1,
                Confidence = i.Confidence
            }).ToList() ?? new List<IngredientDto>(),
            Etapes = raw.Etapes?.Select((e, idx) => new EtapeDto
            {
                Id = Guid.NewGuid(),
                NumeroEtape = e.NumeroEtape > 0 ? e.NumeroEtape : idx + 1,
                Description = e.Description ?? string.Empty,
                TimestampSecondes = e.TimestampSecondes
            }).ToList() ?? new List<EtapeDto>()
        };
    }

    private static string ConstruirePrompt() => """
        Tu es un assistant culinaire. En regardant cette vidéo de cuisine, crée une fiche recette structurée en JSON.
        Ne reproduis pas mot pour mot un texte existant — formule les étapes avec tes propres mots.
        Retourne UNIQUEMENT un JSON valide (sans balises markdown, sans texte avant ou après) avec ce format :
        {
          "titre": "nom de la recette",
          "description": "une courte description originale",
          "tempsPreparation": 30,
          "tempsCuisson": 20,
          "nombrePortions": 4,
          "difficulte": "Facile",
          "typeCuisine": "Française",
          "ingredients": [{"nom": "farine", "quantite": "250", "unite": "Grammes", "confidence": "low"}],
          "etapes": [{"numeroEtape": 1, "description": "Décrire l'étape avec tes propres mots...", "timestampSecondes": 45}]
        }
        Valeurs possibles pour difficulte : Facile, Moyen, Difficile.
        Valeurs possibles pour unite : Tasse, Grammes, Kilogrammes, Millilitres, Litres, CuillereSoupe, CuillereThe, Livres, Onces, Pincee, Unite, AuGout.
        Si une information n'est pas visible, utilise null pour les chaînes ou une valeur par défaut pour les entiers (nombrePortions=4, temps=0).
        Pour "confidence" : ajoute "low" UNIQUEMENT quand la quantité d'un ingrédient n'est ni énoncée à voix haute ni affichée à l'écran, et que tu dois l'estimer toi-même. N'invente jamais une quantité précise sans le signaler — dans ce cas, indique ta meilleure estimation mais marque-la "low" pour que l'utilisateur la vérifie. Si la quantité est clairement donnée dans la vidéo, omets ce champ (ou laisse-le null).
        Pour "timestampSecondes" : indique le moment (en secondes depuis le début de la vidéo) où l'étape est réalisée, si tu peux l'identifier. Sinon, laisse null.
        """;

    private static UniteIngredient ParseUnite(string? unite) => unite?.ToLower() switch
    {
        "tasse" => UniteIngredient.Tasse,
        "grammes" or "g" or "gr" => UniteIngredient.Grammes,
        "kilogrammes" or "kg" => UniteIngredient.Kilogrammes,
        "millilitres" or "ml" => UniteIngredient.Millilitres,
        "litres" or "l" => UniteIngredient.Litres,
        "cuilleresoup" or "cuilleresoupe" or "c. à soupe" or "tbsp" => UniteIngredient.CuillereSoupe,
        "cuillerethe" or "cuillereté" or "c. à thé" or "tsp" => UniteIngredient.CuillereThe,
        "livres" or "lbs" or "lb" => UniteIngredient.Livres,
        "onces" or "oz" => UniteIngredient.Onces,
        "pincee" or "pincée" or "pinch" => UniteIngredient.Pincee,
        "augout" or "au goût" => UniteIngredient.AuGout,
        _ => UniteIngredient.Unite
    };

    // Désérialisation de la réponse Gemini generateContent
    private class ReponseGemini { public List<CandidatGemini>? Candidates { get; set; } }
    private class CandidatGemini { public ContenuGemini? Content { get; set; } public string? FinishReason { get; set; } }
    private class ContenuGemini { public List<PartieGemini>? Parts { get; set; } }
    private class PartieGemini { public string? Text { get; set; } }

    // Désérialisation des ressources de la File API
    private class FichierGeminiEnveloppe { public FichierGemini? File { get; set; } }
    private class FichierGemini
    {
        public string? Name { get; set; }
        public string? Uri { get; set; }
        public string? State { get; set; }
    }

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

    private class IngredientRaw { public string? Nom { get; set; } public string? Quantite { get; set; } public string? Unite { get; set; } public string? Confidence { get; set; } }
    private class EtapeRaw { public int NumeroEtape { get; set; } public string? Description { get; set; } public int? TimestampSecondes { get; set; } }
}
