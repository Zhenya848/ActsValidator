namespace UserService.Application.Commands.VerifyEmail;

public record VerifyEmailCommand(Guid UserId, string Token);