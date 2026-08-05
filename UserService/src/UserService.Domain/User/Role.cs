using Microsoft.AspNetCore.Identity;

namespace UserService.Domain.User;

public class Role : IdentityRole<Guid>
{
    public List<RolePermission> RolePermissions { get; }
}