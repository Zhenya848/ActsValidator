using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UserService.Presentation.Grpc.Interceptors;
using UserService.Presentation.Options;

namespace UserService.Presentation;

public static class Inject
{
    public static IServiceCollection AddFromPresentation(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.Configure<AuthOptions>(config.GetSection(AuthOptions.Auth));
        services.Configure<MessageBrokerOptions>(config.GetSection(MessageBrokerOptions.MessageBroker));
        
        services.AddOptions<AuthOptions>();
        services.AddOptions<MessageBrokerOptions>();
        
        services.AddSingleton<IsServiceInterceptor>();
        
        services.AddGrpc(options =>
        {
            options.Interceptors.Add<IsServiceInterceptor>();
        });
        
        return services;
    }
}