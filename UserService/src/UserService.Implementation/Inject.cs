using Microsoft.Extensions.DependencyInjection;
using UserService.Contracts;

namespace UserService.Implementation;

public static class Inject
{
    public static IServiceCollection AddFromImplementation(
        this IServiceCollection services)
    {
        return services.AddScoped<IAccountsContract, AccountsContract>();
    }
}