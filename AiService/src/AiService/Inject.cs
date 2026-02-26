using System.Security.Cryptography;
using AiService.Authorization;
using AiService.Consumers;
using AiService.Models.ValueObjects.Options;
using AiService.Providers;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace AiService;

public static class Inject
{
    public static IServiceCollection AddFromAiService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient();
        
        services.Configure<AiOptions>(configuration.GetSection(AiOptions.Ai));
        
        services.AddOptions<AiOptions>();
        
        var authOptions = configuration.GetSection(AuthOptions.Auth).Get<AuthOptions>()
                          ?? throw new ApplicationException("Auth options not found");
        
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            var rsa = RSA.Create();
    
            byte[] publicKeyBytes = File.ReadAllBytes(authOptions.PublicKeyPath);
            rsa.ImportRSAPublicKey(publicKeyBytes, out _);
        
            var key = new RsaSecurityKey(rsa);

            options.TokenValidationParameters = TokenValidationParametersFactory
                .CreateWithLifeTime(key);
        });
        
        services.AddSingleton<AiProvider>();
        
        services.Configure<MessageBrokerOptions>(
            configuration.GetSection(MessageBrokerOptions.MessageBroker));
        
        services.AddMassTransit(configure =>
        {
            configure.SetKebabCaseEndpointNameFormatter();
            configure.AddConsumer<SendToAiCommandConsumer>();
            
            var options = configuration.GetSection(MessageBrokerOptions.MessageBroker).Get<MessageBrokerOptions>()
                          ?? throw new ApplicationException("Missing RabbitMQ configuration");
            
            configure.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(new Uri(options.Host),h =>
                {
                    h.Username(options.Username);
                    h.Password(options.Password);
                });
                
                cfg.ConfigureEndpoints(context);
            });
        });
        
        return services;
    }
}