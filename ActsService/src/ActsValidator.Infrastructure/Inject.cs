using ActsValidator.Application.Abstractions;
using ActsValidator.Application.Providers;
using ActsValidator.Application.Repositories;
using ActsValidator.Infrastructure.DbContexts;
using ActsValidator.Infrastructure.Providers;
using ActsValidator.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ActsValidator.Infrastructure;

public static class Inject
{
    public static IServiceCollection AddFromInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<AppDbContext>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ICollationRepository, CollationRepository>();
        services.AddSingleton<IFileProvider, ExcelProvider>();
        
        return services;
    }
}