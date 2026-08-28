using ChatService.Abstractions;
using ChatService.Grpc.Interceptors;
using ChatService.Grpc.Services;
using ChatService.Options;
using ChatService.Providers;
using ChatService.Workers.Outbox;
using Quartz;

namespace ChatService;

public static class Inject
{
    public static IServiceCollection AddServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AuthOptions>(configuration.GetSection(AuthOptions.Auth));
        services.Configure<SupportEmailsOptions>(configuration.GetSection(SupportEmailsOptions.SupportEmails));
        
        services.AddOptions<AuthOptions>();
        services.AddOptions<SupportEmailsOptions>();
        
        services.AddSingleton<ProvideSecretKeyInterceptor>();
        
        services.AddGrpcClient<Greeter.GreeterClient>(options =>
            {
                options.Address = new Uri("http://userservice-api:8081");
            })
            .AddInterceptor<ProvideSecretKeyInterceptor>();
        
        services.AddScoped<IGreeterService, GreeterService>();

        services.AddSingleton<SupportEmailsProvider>();
        
        services.AddQuartz(c =>
        {
            var jobKey = new JobKey(nameof(ProcessOutboxMessagesJob));

            c.AddJob<ProcessOutboxMessagesJob>(jobKey)
                .AddTrigger(t => t.ForJob(jobKey)
                    .WithSimpleSchedule(s => s.WithIntervalInSeconds(3).RepeatForever()));
        });
        
        return services;
    }
}