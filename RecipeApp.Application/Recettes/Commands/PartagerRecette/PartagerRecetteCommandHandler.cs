using MediatR;
using RecipeApp.Domain.Interfaces;

namespace RecipeApp.Application.Recettes.Commands.PartagerRecette;

public class PartagerRecetteCommandHandler : IRequestHandler<PartagerRecetteCommand, Unit>
{
    private readonly IRecetteRepository _recetteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PartagerRecetteCommandHandler(IRecetteRepository recetteRepository, IUnitOfWork unitOfWork)
    {
        _recetteRepository = recetteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(PartagerRecetteCommand commande, CancellationToken annulation)
    {
        var recette = await _recetteRepository.ObtenirParIdAsync(commande.RecetteId, annulation)
            ?? throw new KeyNotFoundException($"Recette {commande.RecetteId} introuvable.");

        if (recette.AuteurId != commande.AuteurId)
            throw new UnauthorizedAccessException("Seul l'auteur peut partager cette recette.");

        await _recetteRepository.PartagerAvecAsync(commande.RecetteId, commande.UtilisateurId, annulation);
        await _unitOfWork.SauvegarderAsync(annulation);

        return Unit.Value;
    }
}