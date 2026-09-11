using System.Security.Cryptography;
using ChatService.Abstractions;
using ChatService.Authorization;
using ChatService.DbContexts;
using ChatService.EmailSender;
using ChatService.EmailSendingOutbox;
using ChatService.Hosts;
using ChatService.Options;
using ChatService.Providers;
using ChatService.Seeding;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Quartz;

namespace ChatService;

public static class Inject
{
    public static IServiceCollection AddServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<AppDbContext>();
        
        services.Configure<SupportEmailsOptions>(configuration.GetSection(SupportEmailsOptions.SupportEmails));
        services.Configure<MailOptions>(configuration.GetSection(MailOptions.SECTION_NAME));
        
        services.AddOptions<MailOptions>();
        services.AddOptions<SupportEmailsOptions>();

        services.AddSingleton<IEmailSender, EmailSender.EmailSender>();
        
        services.AddSingleton<SupportEmailsProvider>();
        services.AddHostedService<SupportEmailsInitializer>();
        
        services.AddSingleton<SupportEmailsSeeder>();
        services.AddScoped<SupportEmailsSeederService>();
        
        services.AddScoped<ProcessOutboxMessagesService>();
        
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
            
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken =
                        context.Request.Query["access_token"];

                    var path = context.HttpContext.Request.Path;

                    if (!string.IsNullOrEmpty(accessToken) &&
                        path.StartsWithSegments("/hubs/chat"))
                    {
                        context.Token = accessToken;
                    }

                    return Task.CompletedTask;
                }
            };
        });
        
        services.AddQuartz(c =>
        {
            var jobKey = new JobKey(nameof(ProcessOutboxMessagesJob));

            c.AddJob<ProcessOutboxMessagesJob>(jobKey)
                .AddTrigger(t => t.ForJob(jobKey)
                    .WithSimpleSchedule(s => s.WithIntervalInSeconds(3).RepeatForever()));
        });
        
        services.AddQuartzHostedService(o => { o.WaitForJobsToComplete = true; });
        
        services.AddSignalR();
        
        return services;
    }
}