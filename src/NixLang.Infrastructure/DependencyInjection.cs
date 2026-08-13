using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NixLang.Application.Common.Interfaces;
using NixLang.Application.Common.Models;
using NixLang.Domain.Repositories;
using NixLang.Infrastructure.Persistence;
using NixLang.Infrastructure.Persistence.Repositories;
using NixLang.Infrastructure.Security;

namespace NixLang.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<NixLangDbContext>((sp, options) =>
        {
            var useSqlite = configuration["UseSqlite"] == "true";
            if (useSqlite)
            {
                var connection = sp.GetRequiredService<System.Data.Common.DbConnection>();
                options.UseSqlite(connection);
            }
            else
            {
                options
                    .UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
                    .UseSnakeCaseNamingConvention();
            }
        });

        // Register repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ILessonRepository, LessonRepository>();
        services.AddScoped<IFavoriteRepository, FavoriteRepository>();
        services.AddScoped<ILessonProgressRepository, LessonProgressRepository>();

        // Register Unit of Work mapped to DbContext
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<NixLangDbContext>());

        // Register Security services
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Register JWT settings and services
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        return services;
    }
}
