using System.Reflection;
using Elastic.CommonSchema.Serilog;
using Elastic.Ingest.Elasticsearch;
using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Serilog.Sinks;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Serilog;
using Serilog.Events;
using UserService.Application.Abstractions;
using UserService.Application.Models;
using UserService.Application.Repositories;
using UserService.Domain;
using UserService.Domain.Shared;
using UserService.Infrastructure.Authorization;
using UserService.Infrastructure.Consumers;
using UserService.Infrastructure.DbContexts;
using UserService.Infrastructure.EmailSender;
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
                
                cfg.ReceiveEndpoint("product-was-bought", e =>
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
        
        string indexFormat =
            $"{Assembly.GetExecutingAssembly().GetName().Name?.ToLower().Replace(".", "-")}-{DateTime.UtcNow:yyyy-MM-dd}";

        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.Debug()
            .WriteTo.Elasticsearch(
                [new Uri(configuration.GetConnectionString("Elasticsearch") 
                         ?? throw new ApplicationException("Elasticsearch connection string not found."))],
                options =>
                {
                    options.DataStream = new DataStreamName(indexFormat);
                    options.TextFormatting = new EcsTextFormatterConfiguration<LogEventEcsDocument>();
                    options.BootstrapMethod = BootstrapMethod.Silent;
                })
            .MinimumLevel.Override("Microsoft.AspNetCore.Hosting", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore.Mvc", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore.Routing", LogEventLevel.Warning)
            .CreateLogger();
        
        services.AddSerilog();
        
        services.AddOpenTelemetry()
            .WithMetrics(c => c
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("ActsService.API"))
                .AddMeter("ActsService")
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddProcessInstrumentation()
                .AddPrometheusExporter());
        
        services.AddScoped<IEmailSender, EmailSender.EmailSender>();
        
        return services;
    }
}