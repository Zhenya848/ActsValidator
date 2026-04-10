using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using UserService.Application.Abstractions;
using UserService.Application.Models;
using UserService.Application.Repositories;
using UserService.Domain;
using UserService.Domain.Shared;
using UserService.Infrastructure.Authorization;
using UserService.Infrastructure.Consumers;
using UserService.Infrastructure.DbContexts;
using UserService.Infrastructure.Repositories;
using UserService.Presentation.Options;

namespace UserService.Infrastructure;

public static class Inject
{
    public static IServiceCollection AddFromInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.AddScoped<ISqlConnectionFactory, SqlConnectionFactory>();
        
        services.Configure<MailOptions>(configuration.GetSection(MailOptions.SECTION_NAME));
        services.AddOptions<MailOptions>();
        
        services.AddDbContext<AuthDbContext>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<ITokenProvider, JwtTokenProvider>();
        
        services.AddIdentity<User, Role>(options =>
        {
            options.User.AllowedUserNameCharacters = UserConstants.AllowedUsernameCharacters;
            options.User.RequireUniqueEmail = true;
            
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = false;
        })
        .AddEntityFrameworkStores<AuthDbContext>()
        .AddDefaultTokenProviders();
        
        var authOptions = configuration.GetSection(AuthOptions.Auth).Get<AuthOptions>()
                          ?? throw new ApplicationException("Auth options not found");
    
        var rsaKeyProvider = new RsaKeyProvider(authOptions);
        services.AddSingleton<IKeyProvider>(rsaKeyProvider);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            var rsaKey = rsaKeyProvider.GetPublicRsa();
            var key = new RsaSecurityKey(rsaKey);

            options.TokenValidationParameters = TokenValidationParametersFactory
                .CreateWithLifeTime(key);
        });
        
        services.AddMassTransit(configure =>
        {
            configure.SetKebabCaseEndpointNameFormatter();
            configure.AddConsumer<ProductWasBoughtConsumer>();
            
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
                
                cfg.ReceiveEndpoint(e =>
                {
                    e.ConfigureConsumer<ProductWasBoughtConsumer>(context);
                    
                    e.UseMessageRetry(r => 
                    {
                        r.Interval(3, TimeSpan.FromSeconds(5));
                        r.Exponential(5, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(10));
                    });
                });
            });
        });
        
        services.AddScoped<IEmailSender, EmailSender>();
        
        return services;
    }
}