using System.Reflection;
using System.Security.Cryptography;
using ActsValidator.Application.Abstractions;
using ActsValidator.Application.Providers;
using ActsValidator.Application.Repositories;
using ActsValidator.Infrastructure.Authorization;
using ActsValidator.Infrastructure.DbContexts;
using ActsValidator.Infrastructure.Providers;
using ActsValidator.Infrastructure.Repositories;
using ActsValidator.Presentation.Options;
using Elastic.CommonSchema.Serilog;
using Elastic.Ingest.Elasticsearch;
using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Serilog.Sinks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Serilog;
using Serilog.Events;

namespace ActsValidator.Infrastructure;

public static class Inject
{
    public static IServiceCollection AddFromInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<ISqlConnectionFactory, SqlConnectionFactory>();
        services.AddScoped<AppDbContext>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IAppRepository, AppRepository>();
        services.AddSingleton<IFileProvider, ExcelProvider>();
        
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
        
        return services;
    }
}