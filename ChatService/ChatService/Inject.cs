namespace ChatService;

public static class Inject
{
    public static IServiceCollection AddServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services;
    }
}