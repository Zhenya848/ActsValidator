using Microsoft.EntityFrameworkCore;
using UserService.Contracts;
using UserService.Infrastructure.DbContexts;

namespace UserService.Implementation;

public class AccountsContract : IAccountsContract
{
    private readonly AuthDbContext _accountsDbContext;

    public AccountsContract(AuthDbContext accountsDbContext)
    {
        _accountsDbContext = accountsDbContext;
    }
    
    public async Task<IReadOnlyList<string>> GetUserPermissionCodes(Guid userId)
    {
        var permissions = await _accountsDbContext.Users
            .Include(u => u.Roles)
            .Where(u => u.Id == userId)
            .SelectMany(u => u.Roles)
            .SelectMany(r => r.RolePermissions)
            .Select(rp => rp.Permission.Code)
            .ToListAsync();

        return permissions;
    }
}