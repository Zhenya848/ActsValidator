using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using UserService.Infrastructure.Options;

namespace UserService.Infrastructure.Seeding;

public class AccountsSeeder(IServiceScopeFactory serviceScopeFactory)
{
    public async Task SeedAsync()
    {
        using var scope = serviceScopeFactory.CreateScope();
        
        var service = scope.ServiceProvider.GetRequiredService<AccountsSeederService>();
        var adminOptions = scope.ServiceProvider.GetRequiredService<IOptions<AdminOptions>>().Value;
        
        await service.SeedAsync(adminOptions);
    }
}