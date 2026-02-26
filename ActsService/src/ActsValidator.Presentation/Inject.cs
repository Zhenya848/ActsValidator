using ActsValidator.Presentation.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ActsValidator.Presentation;

public static class Inject
{
    public static IServiceCollection AddFromPresentation(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.Configure<AuthOptions>(config.GetSection(AuthOptions.Auth));
        
        services.AddOptions<AuthOptions>();
        
        return services;
    }
}