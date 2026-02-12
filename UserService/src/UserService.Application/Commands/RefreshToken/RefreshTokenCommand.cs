namespace UserService.Application.Commands.RefreshToken;

public record RefreshTokenCommand(string AccessToken, Guid RefreshToken);