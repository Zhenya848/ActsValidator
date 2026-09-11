using ChatService.Providers;

namespace ChatService.Hosts;

public class SupportEmailsInitializer : IHostedService
{
    private readonly SupportEmailsProvider _provider;

    public SupportEmailsInitializer(SupportEmailsProvider provider)
    {
        _provider = provider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return _provider.InitializeAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}