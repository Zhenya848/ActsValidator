namespace UserService.Domain.Shared;

public record LoginUserResponse(string AccessToken, Guid RefreshToken, UserInfo User);