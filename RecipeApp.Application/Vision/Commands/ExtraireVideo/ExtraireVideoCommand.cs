using MediatR;
using RecipeApp.Application.DTOs;

namespace RecipeApp.Application.Vision.Commands.ExtraireVideo;

/// <summary>Extrait une recette depuis une vidéo (fichier uploadé) ou une URL YouTube. Un seul des deux modes doit être renseigné.</summary>
public record ExtraireVideoCommand : IRequest<RecetteExtraiteDto>
{
    public Stream? Flux { get; init; }
    public string? TypeMime { get; init; }
    public long TailleOctets { get; init; }

    public string? UrlYoutube { get; init; }
}
