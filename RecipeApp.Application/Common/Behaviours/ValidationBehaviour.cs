using FluentValidation;
using MediatR;

namespace RecipeApp.Application.Common.Behaviours;

/// <summary>Comportement MediatR qui valide automatiquement chaque commande avant son traitement.</summary>
public class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validateurs;

    public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validateurs)
    {
        _validateurs = validateurs;
    }

    public async Task<TResponse> Handle(TRequest requete, RequestHandlerDelegate<TResponse> suivant, CancellationToken annulation)
    {
        if (_validateurs.Any())
        {
            var contexte = new ValidationContext<TRequest>(requete);
            var resultats = await Task.WhenAll(_validateurs.Select(v => v.ValidateAsync(contexte, annulation)));
            var erreurs = resultats.SelectMany(r => r.Errors).Where(e => e != null).ToList();

            if (erreurs.Count > 0)
                throw new ValidationException(erreurs);
        }

        return await suivant();
    }
}