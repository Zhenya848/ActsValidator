using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Domain;
using UserService.Domain.User;
using UserService.Infrastructure.DbContexts;
using UserService.Infrastructure.Options;

namespace UserService.Infrastructure.Seeding;

public class AccountsSeederService(
    UserManager<User> userManager, 
    RoleManager<Role> roleManager,
    AuthDbContext accountsDbContext,
    ILogger<AccountsSeederService> logger)
{
    public async Task SeedAsync(AdminOptions adminOptions)
    {
        var json = await File.ReadAllTextAsync("etc/accounts.json");

        var seedData = JsonSerializer.Deserialize<RolePermissionOptions>(json)
                       ?? throw new ApplicationException("Role Permission Config is missing");

        await SeedPermissionsAsync(seedData);
        
        logger.LogInformation("Seeded permission successfully");
        
        await SeedRolesAsync(seedData);
        
        logger.LogInformation("Seeded roles successfully");

        await accountsDbContext.SaveChangesAsync();
        
        logger.LogInformation("Saved permissions and roles successfully");
        
        await SeedRolePermissionsAsync(seedData);
        
        logger.LogInformation("Seeded role_permissions successfully");
        
        var adminRole = await roleManager.FindByNameAsync(AdminAccount.ADMIN)
            ?? throw new ApplicationException("Admin Role not found");
        
        var adminUser = await userManager.Users
            .FirstOrDefaultAsync(u => u.Roles.Any(r => r.Name == adminRole.Name));

        if (adminUser == null)
        {
            adminUser = User.CreateAdmin(adminOptions.Name, adminOptions.Email, adminRole);
        
            await userManager.CreateAsync(adminUser, adminOptions.Password);
        }
        
        logger.LogInformation("Created user admin successfully");
        
        var adminExist = await accountsDbContext.AdminAccounts
            .AnyAsync(e => e.User.Email == adminOptions.Email);

        if (adminExist == false)
        {
            var adminAccount = AdminAccount.CreateAdmin(adminUser);
            await accountsDbContext.AdminAccounts.AddAsync(adminAccount);
        }
        
        logger.LogInformation("Created admin account successfully");
        
        await accountsDbContext.SaveChangesAsync();
        
        logger.LogInformation("Saved all data successfully");
    }
    
    private async Task SeedPermissionsAsync(RolePermissionOptions seedData)
    {
        var keys = seedData.Permissions.Keys.ToList();
        var permissionsToAdd = new List<string>();
        
        foreach (var key in keys)
        {
            var permissions = seedData.Permissions[key];

            permissionsToAdd.AddRange(permissions);
        }
        
        foreach (var permissionCode in permissionsToAdd)
        {
            var isPermissionExist = await accountsDbContext.Permissions.AnyAsync(p => p.Code == permissionCode);

            if (isPermissionExist == false)
                await accountsDbContext.Permissions.AddAsync(new Permission() 
                    { Code = permissionCode, Description = permissionCode} );
        }
    }
    
    private async Task SeedRolesAsync(RolePermissionOptions seedData)
    {
        foreach (var role in seedData.Roles.Keys)
        {
            var isRoleExist = await roleManager.FindByNameAsync(role);

            if (isRoleExist is null)
                await roleManager.CreateAsync(new Role() { Name = role });
        }
    }

    private async Task SeedRolePermissionsAsync(RolePermissionOptions seedData)
    {
        foreach (var roleName in seedData.Roles.Keys)
        {
            var roleExist = await roleManager.FindByNameAsync(roleName);
            
            if (roleExist is null)
                throw new ApplicationException($"Role {roleName} does not exist");

            foreach (var permissionCode in seedData.Roles[roleName])
            {
                var permissionExist = await accountsDbContext.Permissions
                    .FirstOrDefaultAsync(p => p.Code == permissionCode);

                var rolePermission = await accountsDbContext.RolePermissions.AnyAsync(rp =>
                    rp.RoleId == roleExist.Id && rp.PermissionId == permissionExist!.Id);
                
                if (rolePermission == false)
                    await accountsDbContext.RolePermissions.AddAsync(new RolePermission() 
                        { RoleId = roleExist.Id, PermissionId = permissionExist!.Id });
            }
        }
    }
}