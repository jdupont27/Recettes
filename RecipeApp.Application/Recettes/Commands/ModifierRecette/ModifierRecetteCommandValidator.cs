using FluentValidation;

namespace RecipeApp.Application.Recettes.Commands.ModifierRecette;

public class ModifierRecetteCommandValidator : AbstractValidator<ModifierRecetteCommand>
{
    public ModifierRecetteCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty().WithMessage("L'identifiant de la recette est obligatoire.");
        RuleFor(c => c.AuteurId).NotEmpty().WithMessage("L'auteur est obligatoire.");

        RuleFor(c => c.Titre)
            .NotEmpty().WithMessage("Le titre est obligatoire.")
            .MaximumLength(200).WithMessage("Le titre ne peut pas dépasser 200 caractères.");

        RuleFor(c => c.NombrePortions)
            .GreaterThan(0).WithMessage("Le nombre de portions doit être supérieur à 0.");

        RuleFor(c => c.Ingredients)
            .NotEmpty().WithMessage("La recette doit contenir au moins un ingrédient.");

        RuleFor(c => c.Etapes)
            .NotEmpty().WithMessage("La recette doit contenir au moins une étape.");
    }
}