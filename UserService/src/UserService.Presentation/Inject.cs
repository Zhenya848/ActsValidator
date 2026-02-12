using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UserService.Presentation.Options;

namespace UserService.Presentation;

public static class Inject
{
    public static IServiceCollection AddFromPresentation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AuthOptions>(
            configuration.GetSection(AuthOptions.Auth));

        services.AddOptions<AuthOptions>();
        
        return services;
    }
}