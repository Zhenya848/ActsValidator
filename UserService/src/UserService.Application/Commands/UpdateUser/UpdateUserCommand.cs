namespace UserService.Application.Commands.UpdateUser;

public record UpdateUserCommand(Guid UserId, string UserName, string Email, string? Password, string? NewPassword);