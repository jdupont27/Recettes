using MediatR;
using RecipeApp.Application.DTOs;
using RecipeApp.Application.Interfaces;

namespace RecipeApp.Application.Vision.Commands.ExtraireVideo;

public class ExtraireVideoCommandHandler : IRequestHandler<ExtraireVideoCommand, RecetteExtraiteDto>
{
    private readonly IServiceVisionVideo _serviceVisionVideo;

    public ExtraireVideoCommandHandler(IServiceVisionVideo serviceVisionVideo)
    {
        _serviceVisionVideo = serviceVisionVideo;
    }

    public async Task<RecetteExtraiteDto> Handle(ExtraireVideoCommand commande, CancellationToken annulation)
    {
        if (!string.IsNullOrEmpty(commande.UrlYoutube))
            return await _serviceVisionVideo.ExtraireRecetteDeUrlYoutubeAsync(commande.UrlYoutube, annulation);

        return await _serviceVisionVideo.ExtraireRecetteDeFichierAsync(
            commande.Flux!, commande.TypeMime!, commande.TailleOctets, annulation);
    }
}
