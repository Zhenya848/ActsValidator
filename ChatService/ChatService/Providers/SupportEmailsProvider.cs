using ChatService.Abstractions;

namespace ChatService.Providers;

public class SupportEmailsProvider
{
    private readonly IServiceScopeFactory _scopeFactory;
    private List<string> _emails = [];
    public IReadOnlyList<string> Emails => _emails;

    public SupportEmailsProvider(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task<IReadOnlyList<string>> GetSupportEmails(CancellationToken cancellationToken = default)
    {
        if (_emails.Count > 0)
            return _emails;

        using var scope = _scopeFactory.CreateScope();
        var greeterService = scope.ServiceProvider.GetRequiredService<IGreeterService>();

        var result = await greeterService.GetUsersByPermissionsAsync(["chat.all"], cancellationToken);

        _emails = result
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        
        return result;
    }
}