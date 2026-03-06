using MediatR;
using RecipeApp.Application.DTOs;
using RecipeApp.Domain.Interfaces;

namespace RecipeApp.Application.Recettes.Queries.ObtenirCategories;

public class ObtenirCategoriesQueryHandler : IRequestHandler<ObtenirCategoriesQuery, List<CategorieDto>>
{
    private readonly ICategorieRepository _categorieRepository;

    public ObtenirCategoriesQueryHandler(ICategorieRepository categorieRepository)
    {
        _categorieRepository = categorieRepository;
    }

    public async Task<List<CategorieDto>> Handle(ObtenirCategoriesQuery requete, CancellationToken annulation)
    {
        var categories = await _categorieRepository.ObtenirToutesAsync(annulation);
        return categories.Select(c => new CategorieDto { Id = c.Id, Nom = c.Nom }).ToList();
    }
}
