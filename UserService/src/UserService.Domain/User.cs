using Microsoft.AspNetCore.Identity;

namespace UserService.Domain;

public class User : IdentityUser<Guid>
{
    public string DisplayName { get; init; }
}