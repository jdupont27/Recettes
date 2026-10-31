using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RecipeApp.Application.Interfaces;

namespace RecipeApp.Infrastructure.Services;

/// <summary>Service d'envoi de courriels via l'API Resend.</summary>
public class ServiceCourrielResend : IServiceCourriel
{
    private readonly HttpClient _httpClient;
    private readonly string _adresseExpediteur;
    private readonly string _nomExpediteur;
    private readonly ILogger<ServiceCourrielResend> _logger;

    public ServiceCourrielResend(HttpClient httpClient, IConfiguration configuration, ILogger<ServiceCourrielResend> logger)
    {
        var cleApi = configuration["Email:ResendApiKey"]
            ?? throw new InvalidOperationException("Email:ResendApiKey manquant.");

        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://api.resend.com/");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", cleApi);

        // Sans domaine vérifié sur Resend, seule leur adresse de test par défaut
        // est autorisée comme expéditeur, et l'envoi est limité à l'adresse du
        // compte Resend lui-même.
        _adresseExpediteur = configuration["Email:FromAddress"] ?? "onboarding@resend.dev";
        _nomExpediteur = configuration["Email:FromName"] ?? "RecetteApp";
        _logger = logger;
    }

    public async Task EnvoyerAsync(string destinataire, string sujet, string corpsHtml, CancellationToken annulation = default)
    {
        var requete = new
        {
            from = $"{_nomExpediteur} <{_adresseExpediteur}>",
            to = new[] { destinataire },
            subject = sujet,
            html = corpsHtml
        };

        var reponse = await _httpClient.PostAsJsonAsync("emails", requete, annulation);

        if (!reponse.IsSuccessStatusCode)
        {
            var contenu = await reponse.Content.ReadAsStringAsync(annulation);
            _logger.LogError("Échec de l'envoi via Resend ({Code}) : {Contenu}", reponse.StatusCode, contenu);
            throw new InvalidOperationException($"Échec de l'envoi du courriel via Resend : {reponse.StatusCode}");
        }

        _logger.LogInformation("Courriel envoyé via Resend à {Destinataire} : {Sujet}", destinataire, sujet);
    }
}
