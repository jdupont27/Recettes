using RecipeApp.Application.DTOs;

namespace RecipeApp.Application.Interfaces;

public interface IServiceVisionVideo
{
    /// <summary>Extrait une recette depuis un fichier vidéo uploadé, via la File API de Gemini.</summary>
    Task<RecetteExtraiteDto> ExtraireRecetteDeFichierAsync(Stream flux, string typeMime, long tailleOctets, CancellationToken annulation = default);

    /// <summary>Extrait une recette depuis une URL YouTube (support natif Gemini, sans upload).</summary>
    Task<RecetteExtraiteDto> ExtraireRecetteDeUrlYoutubeAsync(string urlYoutube, CancellationToken annulation = default);
}
