using ChatService.Options;
using Microsoft.Extensions.Options;

namespace ChatService.Seeding;

public class SupportEmailsSeeder(IServiceScopeFactory serviceScopeFactory)
{
    public async Task SeedAsync()
    {
        using var scope = serviceScopeFactory.CreateScope();
        
        var service = scope.ServiceProvider.GetRequiredService<SupportEmailsSeederService>();
        var adminOptions = scope.ServiceProvider.GetRequiredService<IOptions<SupportEmailsOptions>>().Value;
        
        await service.SeedAsync(adminOptions);
    }
}