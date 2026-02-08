using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ActsValidator.Presentation;

public static class Inject
{
    public static IServiceCollection AddFromPresentation(
        this IServiceCollection services,
        IConfiguration config)
    {
        return services;
    }
}