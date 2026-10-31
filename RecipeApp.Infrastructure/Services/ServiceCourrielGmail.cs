using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using RecipeApp.Application.Interfaces;

namespace RecipeApp.Infrastructure.Services;

/// <summary>Service d'envoi de courriels via le relais SMTP de Gmail.</summary>
public class ServiceCourrielGmail : IServiceCourriel
{
    private readonly string _utilisateur;
    private readonly string _motDePasseApplication;
    private readonly string _nomExpediteur;
    private readonly ILogger<ServiceCourrielGmail> _logger;

    public ServiceCourrielGmail(IConfiguration configuration, ILogger<ServiceCourrielGmail> logger)
    {
        _utilisateur = configuration["Email:SmtpUser"]
            ?? throw new InvalidOperationException("Email:SmtpUser manquant.");
        _motDePasseApplication = configuration["Email:SmtpPassword"]
            ?? throw new InvalidOperationException("Email:SmtpPassword manquant.");
        _nomExpediteur = configuration["Email:FromName"] ?? "RecetteApp";
        _logger = logger;
    }

    public async Task EnvoyerAsync(string destinataire, string sujet, string corpsHtml, CancellationToken annulation = default)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_nomExpediteur, _utilisateur));
        message.To.Add(MailboxAddress.Parse(destinataire));
        message.Subject = sujet;
        message.Body = new BodyBuilder { HtmlBody = corpsHtml }.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls, annulation);
        await client.AuthenticateAsync(_utilisateur, _motDePasseApplication, annulation);
        await client.SendAsync(message, annulation);
        await client.DisconnectAsync(true, annulation);

        _logger.LogInformation("Courriel envoyé à {Destinataire} : {Sujet}", destinataire, sujet);
    }
}
