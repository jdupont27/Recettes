using MediatR;
using RecipeApp.Application.DTOs;

namespace RecipeApp.Application.Recettes.Queries.ObtenirCategories;

public record ObtenirCategoriesQuery : IRequest<List<CategorieDto>>;
