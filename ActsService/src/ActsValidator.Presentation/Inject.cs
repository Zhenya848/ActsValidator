using ActsValidator.Application.Abstractions;
using ActsValidator.Presentation.Grpc.Interceptors;
using ActsValidator.Presentation.Grpc.Services;
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
        
        services.AddSingleton<ProvideSecretKeyInterceptor>();
        
        services.AddGrpcClient<Greeter.GreeterClient>(options =>
        {
            options.Address = new Uri("http://localhost:5171");
        })
        .AddInterceptor<ProvideSecretKeyInterceptor>();
        
        services.AddScoped<IGreeterService, GreeterService>();
        
        return services;
    }
}