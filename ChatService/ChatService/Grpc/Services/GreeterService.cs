using ChatService.Abstractions;

namespace ChatService.Grpc.Services;

public class GreeterService : IGreeterService
{
    private readonly Greeter.GreeterClient _client;
    private readonly ILogger<GreeterService> _logger;

    public GreeterService(Greeter.GreeterClient client, ILogger<GreeterService> logger)
    {
        _client = client;
        _logger = logger;
    }
    
    public async Task<string[]> GetUsersByPermissionsAsync(string[] permissions, CancellationToken cancellationToken)
    {
        try
        {
            var request = new GetUsersByPermissionsRequest() { Permissions = { permissions } };

            var result = await _client
                .GetUsersByPermissionsAsync(request, cancellationToken: cancellationToken);

            return result.UserEmails.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return [];
        }
    }
}