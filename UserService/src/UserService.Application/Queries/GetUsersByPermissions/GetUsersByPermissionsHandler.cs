using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserService.Application.Abstractions;
using UserService.Domain.User;

namespace UserService.Application.Queries.GetUsersByPermissions;

public class GetUsersByPermissionsHandler : IQueryHandler<string[], string[]>
{
    private readonly UserManager<User> _userManager;

    public GetUsersByPermissionsHandler(UserManager<User> userManager)
    {
        _userManager = userManager;
    }
    
    public async Task<string[]> Handle(string[] query, CancellationToken cancellationToken = default)
    {
        var permissions = query
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .ToArray();

        if (permissions.Length == 0)
            return [];

        return await _userManager.Users
            .Where(u =>
                u.Roles
                    .SelectMany(r => r.RolePermissions)
                    .Any(rp => permissions.Contains(rp.Permission.Code)))
            .Select(u => u.Email!)
            .Distinct()
            .ToArrayAsync(cancellationToken);
    }
}