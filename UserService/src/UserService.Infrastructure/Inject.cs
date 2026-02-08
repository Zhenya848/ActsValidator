using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using UserService.Application.EmailSender;
using UserService.Domain;
using UserService.Domain.Shared;
using UserService.Domain.Shared.ValueObjects.EmailSender;
using UserService.Infrastructure.DbContexts;

namespace UserService.Infrastructure;

public static class Inject
{
    public static IServiceCollection AddFromInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.Configure<MailOptions>(configuration.GetSection(MailOptions.SECTION_NAME));
        services.AddOptions<MailOptions>();
        
        services.AddDbContext<AuthDbContext>();
        
        services.AddIdentity<User, Role>(options =>
            {
                options.User.AllowedUserNameCharacters = UserConstants.AllowedUsernameCharacters;
                options.User.RequireUniqueEmail = true;
                
                options.Password.RequiredLength = 4;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
            })
            .AddEntityFrameworkStores<AuthDbContext>()
            .AddDefaultTokenProviders();
        
        services.AddScoped<IEmailSender, EmailSender>();
        
        return services;
    }
}