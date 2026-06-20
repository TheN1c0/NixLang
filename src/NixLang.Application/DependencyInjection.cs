using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NixLang.Application.Common.Behaviors;

namespace NixLang.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        // Register MediatR handlers from this assembly
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        // Register all validators from this assembly
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
