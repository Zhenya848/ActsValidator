namespace UserService.Presentation.Requests;

public record RefreshTokenRequest(string AccessToken, Guid RefreshToken);