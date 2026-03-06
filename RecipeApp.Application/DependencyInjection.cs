using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RecipeApp.Application.Common.Behaviours;

namespace RecipeApp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AjouterCoucheApplication(this IServiceCollection services)
    {
        // Enregistrement MediatR — découverte automatique de tous les handlers de l'assembly
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        // Validation automatique via FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Pipeline MediatR : validation avant chaque handler
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));

        return services;
    }
}
