namespace UserService.Application.Commands.ResetPassword;

public record ResetPasswordCommand(Guid UserId, string Token, string NewPassword);