using ActsValidator.Application.Abstractions;
using ActsValidator.Domain.Shared.ValueObjects.Dtos;
using Dapper;
using Microsoft.Extensions.DependencyInjection;

namespace ActsValidator.Application;

public static class Inject
{
    public static IServiceCollection AddFromApplication(this IServiceCollection services)
    {
        SqlMapper.AddTypeHandler(new JsonTypeHandler<DiscrepancyDto[]>());
        
        var assembly = typeof(Inject).Assembly;
        
        services.Scan(scan => scan.FromAssemblies(assembly)
            .AddClasses(classes => classes.AssignableToAny(
                typeof(ICommandHandler<,>),
                typeof(IQueryHandler<,>)))
            .AsSelfWithInterfaces()
            .WithLifetime(ServiceLifetime.Scoped));

        return services;
    }
}