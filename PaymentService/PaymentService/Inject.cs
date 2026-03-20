using System.Reflection;
using System.Security.Cryptography;
using Application.Abstractions;
using Elastic.CommonSchema.Serilog;
using Elastic.Ingest.Elasticsearch;
using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Serilog.Sinks;
using Framework.Authorization;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using PaymentService.Abstractions;
using PaymentService.DbContexts;
using PaymentService.Models.Shared;
using PaymentService.Options;
using PaymentService.Outbox;
using PaymentService.Seeding;
using Quartz;
using Serilog;
using Serilog.Events;

namespace PaymentService;

public static class Inject
{
    public static IServiceCollection AddServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RabbitMQOptions>(
            configuration.GetSection(RabbitMQOptions.RabbitMQ));

        services.AddOptions<RabbitMQOptions>();
        
        services.AddHttpContextAccessor();
        
        services.AddScoped<AppDbContext>();
        services.AddScoped<ProcessOutboxMessagesService>();
        
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.Configure<YandexKassaOptions>(
            configuration.GetSection(YandexKassaOptions.YANDEX));

        services.AddOptions<YandexKassaOptions>();
        
        services.AddScoped<ProductsSeeder>();
        
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

        services.AddMassTransit(configure =>
        {
            var options = configuration.GetSection(RabbitMQOptions.RabbitMQ).Get<RabbitMQOptions>()
                          ?? throw new ApplicationException("Missing RabbitMQ configuration");

            configure.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(new Uri(options.Host), h =>
                {
                    h.Username(options.Username);
                    h.Password(options.Password);
                });
            });
            
            services.AddControllers();
        });

        services.AddQuartz(c =>
        {
            var jobKey = new JobKey(nameof(ProcessOutboxMessagesJob));

            c.AddJob<ProcessOutboxMessagesJob>(jobKey)
                .AddTrigger(t => t.ForJob(jobKey)
                    .WithSimpleSchedule(s => s.WithIntervalInSeconds(3).RepeatForever()));
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
        
        services.AddOpenTelemetry()
            .WithMetrics(c => c
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("SynapzeAI.API"))
                .AddMeter("SynapzeAI")
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddProcessInstrumentation()
                .AddPrometheusExporter());

        services.AddQuartzHostedService(o => { o.WaitForJobsToComplete = true; });
        
        return services;
    }
}