using MediatR;
using RecipeApp.Application.Interfaces;
using RecipeApp.Domain.Interfaces;

namespace RecipeApp.Application.Recettes.Commands.SupprimerRecette;

public class SupprimerRecetteCommandHandler : IRequestHandler<SupprimerRecetteCommand, Unit>
{
    private readonly IRecetteRepository _recetteRepository;
    private readonly IServiceFichiers _serviceFichiers;
    private readonly IUnitOfWork _unitOfWork;

    public SupprimerRecetteCommandHandler(
        IRecetteRepository recetteRepository,
        IServiceFichiers serviceFichiers,
        IUnitOfWork unitOfWork)
    {
        _recetteRepository = recetteRepository;
        _serviceFichiers = serviceFichiers;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(SupprimerRecetteCommand commande, CancellationToken annulation)
    {
        var recette = await _recetteRepository.ObtenirParIdAsync(commande.Id, annulation)
            ?? throw new KeyNotFoundException($"Recette {commande.Id} introuvable.");

        if (recette.AuteurId != commande.AuteurId)
            throw new UnauthorizedAccessException("Vous n'êtes pas autorisé à supprimer cette recette.");

        // Supprimer l'image associée si elle existe
        if (!string.IsNullOrEmpty(recette.CheminImage))
            await _serviceFichiers.SupprimerImageAsync(recette.CheminImage, annulation);

        await _recetteRepository.SupprimerAsync(commande.Id, annulation);
        await _unitOfWork.SauvegarderAsync(annulation);

        return Unit.Value;
    }
}