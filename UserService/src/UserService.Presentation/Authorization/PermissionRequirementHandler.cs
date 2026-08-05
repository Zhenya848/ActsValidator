using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using UserService.Contracts;
using UserService.Domain.Shared;

namespace UserService.Presentation.Authorization
{
    public class PermissionRequirementHandler(IServiceScopeFactory serviceScopeFactory) 
        : AuthorizationHandler<PermissionAttribute>
    {
        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context, 
            PermissionAttribute permission)
        {
            var userIdString = context.User.Claims
                .FirstOrDefault(c => c.Type == CustomClaims.Sub)
                ?.Value;
            
            if (userIdString is null || Guid.TryParse(userIdString, out var userId) == false)
                return;
            
            var scope = serviceScopeFactory.CreateScope();
            var accountContract = scope.ServiceProvider.GetRequiredService<IAccountsContract>();

            var permissions = await accountContract.GetUserPermissionCodes(userId);
            
            if (permissions.Contains(permission.Code))
                 context.Succeed(permission);
        }
    }
}
