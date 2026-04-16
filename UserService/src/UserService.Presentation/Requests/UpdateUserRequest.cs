namespace UserService.Presentation.Requests;

public record UpdateUserRequest(string UserName, string Email, string? Password, string? NewPassword);