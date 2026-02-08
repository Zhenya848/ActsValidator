using Microsoft.AspNetCore.Mvc;
using UserService.Application.Commands.RegisterUser;
using UserService.Application.Commands.VerifyEmail;
using UserService.Presentation.Extensions;
using UserService.Presentation.Requests;

namespace UserService.Presentation;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Handle(
        [FromBody] RegisterUserRequest request,
        [FromServices] RegisterUserHandler handler,
        CancellationToken cancellationToken = default)
    {
        var command = new RegisterUserCommand(request.UserName, request.Email, request.Password);
        
        var result = await handler.Handle(command, cancellationToken);

        if (result.IsFailure)
            return result.Error.ToResponse();

        return Ok(Envelope.Ok(result.Value));
    }

    [HttpGet("email-verification")]
    public async Task<IActionResult> VerifyEmail(
        [FromQuery] Guid userId,
        [FromQuery] string token,
        [FromServices] VerifyEmailHandler handler,
        CancellationToken cancellationToken = default)
    {
        var command = new VerifyEmailCommand(userId, token);
        
        var result = await handler.Handle(command, cancellationToken);
        
        if (result.IsFailure)
            return result.Error.ToResponse();
        
        return Ok(Envelope.Ok(result.Value));
    }
}