namespace ChatService.Abstractions;

public interface IGreeterService
{
    public Task<string[]> GetUsersByPermissionsAsync(string[] permissions, CancellationToken cancellationToken);
}