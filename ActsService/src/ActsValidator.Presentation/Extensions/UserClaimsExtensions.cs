using System.Security.Claims;
using ActsValidator.Domain.Shared;

namespace ActsValidator.Presentation.Extensions;

public static class UserClaimsExtensions
{
    public static Guid GetUserIdRequired(this ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(CustomClaims.Sub);

        if (userIdClaim is null || Guid.TryParse(userIdClaim.Value, out var userId) == false)
            throw new UnauthorizedAccessException("User ID not found in claims");

        return userId;
    }
    
    public static Guid? GetUserId(this ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(CustomClaims.Sub);

        if (userIdClaim is null || Guid.TryParse(userIdClaim.Value, out var userId) == false)
            return null;

        return userId;
    }

    public static string GetUserEmailRequired(this ClaimsPrincipal user)
    {
        return user.FindFirst(CustomClaims.Email)?.Value
               ?? throw new UnauthorizedAccessException("User email not found in claims");
    }
    
    public static string? GetUserEmail(this ClaimsPrincipal user)
    {
        return user.FindFirst(CustomClaims.Email)?.Value;
    }

    public static string GetUserNameRequired(this ClaimsPrincipal user)
    {
        return user.FindFirst(CustomClaims.Name)?.Value 
               ?? throw new UnauthorizedAccessException("User name not found in claims");
    }
    
    public static string? GetUserName(this ClaimsPrincipal user)
    {
        return user.FindFirst(CustomClaims.Name)?.Value;
    }
}