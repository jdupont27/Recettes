using FluentValidation;

namespace RecipeApp.Application.Recettes.Commands.CreerRecette;

public class CreerRecetteCommandValidator : AbstractValidator<CreerRecetteCommand>
{
    public CreerRecetteCommandValidator()
    {
        RuleFor(c => c.Titre)
            .NotEmpty().WithMessage("Le titre est obligatoire.")
            .MaximumLength(200).WithMessage("Le titre ne peut pas dépasser 200 caractères.");

        RuleFor(c => c.TempsPreparation)
            .GreaterThanOrEqualTo(0).WithMessage("Le temps de préparation ne peut pas être négatif.");

        RuleFor(c => c.TempsCuisson)
            .GreaterThanOrEqualTo(0).WithMessage("Le temps de cuisson ne peut pas être négatif.");

        RuleFor(c => c.NombrePortions)
            .GreaterThan(0).WithMessage("Le nombre de portions doit être supérieur à 0.");

        RuleFor(c => c.AuteurId)
            .NotEmpty().WithMessage("L'auteur est obligatoire.");

        RuleFor(c => c.Ingredients)
            .NotEmpty().WithMessage("La recette doit contenir au moins un ingrédient.");

        RuleFor(c => c.Etapes)
            .NotEmpty().WithMessage("La recette doit contenir au moins une étape.");

        RuleForEach(c => c.Ingredients).ChildRules(ingredient =>
        {
            ingredient.RuleFor(i => i.Nom)
                .NotEmpty().WithMessage("Le nom de l'ingrédient est obligatoire.");
            ingredient.RuleFor(i => i.Quantite)
                .GreaterThanOrEqualTo(0).WithMessage("La quantité ne peut pas être négative.");
        });

        RuleForEach(c => c.Etapes).ChildRules(etape =>
        {
            etape.RuleFor(e => e.Description)
                .NotEmpty().WithMessage("La description de l'étape est obligatoire.");
        });
    }
}