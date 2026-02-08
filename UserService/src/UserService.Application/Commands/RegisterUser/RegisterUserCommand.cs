namespace UserService.Application.Commands.RegisterUser;

public record RegisterUserCommand(string UserName, string Email, string Password);