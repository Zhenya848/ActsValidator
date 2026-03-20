using System.Net.Mail;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.Commands.LoginUser;
using UserService.Application.Commands.LogoutUser;
using UserService.Application.Commands.RefreshToken;
using UserService.Application.Commands.RegisterUser;
using UserService.Application.Commands.SendVerificationCode;
using UserService.Application.Commands.UpdateUser;
using UserService.Application.Commands.VerifyEmail;
using UserService.Application.Queries.GetUser;
using UserService.Domain.Shared;
using UserService.Presentation.Extensions;
using UserService.Presentation.Requests;

namespace UserService.Presentation;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    [HttpPost("registration")]
    public async Task<IActionResult> Register(
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
        
        return Ok(Envelope.Ok(null));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginUserRequest request,
        [FromServices] LoginUserHandler handler,
        CancellationToken cancellationToken = default)
    {
        var command = new LoginUserCommand(request.Email, request.Password);
        
        var result = await handler.Handle(command, cancellationToken);
        
        if (result.IsFailure)
            return result.Error.ToResponse();
        
        HttpContext.Response.Cookies.Append("refreshToken", result.Value.RefreshToken.ToString());
        
        return Ok(Envelope.Ok(result.Value));
    }
    
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        [FromServices] LogoutUserHandler handler, 
        CancellationToken cancellationToken = default)
    {
        if (HttpContext.Request.Cookies.TryGetValue("refreshToken", out var refreshToken) == false)
            return BadRequest("Refresh token was missing!");

        if (Guid.TryParse(refreshToken, out var refreshTokenGuid) == false)
            return Errors.Token.InvalidToken().ToResponse();
        
        var result = await handler.Handle(refreshTokenGuid, cancellationToken);

        if (result.IsFailure)
            return result.Error.ToResponse();
        
        HttpContext.Response.Cookies.Delete("refreshToken");
        
        return Ok();
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken(
        [FromServices] RefreshTokenHandler handler,
        CancellationToken cancellationToken = default)
    {
        if (HttpContext.Request.Cookies.TryGetValue("refreshToken", out var refreshToken) == false)
            return Unauthorized();
        
        if (Guid.TryParse(refreshToken, out var refreshTokenGuid) == false)
            return Errors.Token.InvalidToken().ToResponse();
        
        var result = await handler.Handle(refreshTokenGuid, cancellationToken);
        
        if (result.IsFailure)
            return result.Error.ToResponse();
        
        HttpContext.Response.Cookies.Append("refreshToken", result.Value.RefreshToken.ToString());
        
        return Ok(Envelope.Ok(result.Value));
    }

    [HttpGet("get-user")]
    public async Task<IActionResult> GetUser(
        [FromServices] GetUserHandler handler,
        CancellationToken cancellationToken = default)
    {
        if (HttpContext.Request.Cookies.TryGetValue("refreshToken", out var refreshToken) == false)
            return Unauthorized();
        
        if (Guid.TryParse(refreshToken, out var refreshTokenGuid) == false)
            return Errors.Token.InvalidToken().ToResponse();
        
        var result = await handler.Handle(refreshTokenGuid, cancellationToken);
        
        if (result.IsFailure)
            return result.Error.ToResponse();
        
        return Ok(Envelope.Ok(result.Value));
    }

    [Authorize]
    [HttpPut("update-user")]
    public async Task<IActionResult> UpdateUser(
        [FromBody] UpdateUserRequest request,
        [FromServices] UpdateUserHandler handler,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserIdRequired();
        var command = new UpdateUserCommand(
            userId, request.UserName, request.Email, request.Password, request.OldPassword);
        
        var result = await handler.Handle(command, cancellationToken);
        
        if (result.IsFailure)
            return result.Error.ToResponse();
        
        return Ok(Envelope.Ok(result.Value));
    }

    [Authorize]
    [HttpPost("send-verification-code")]
    public async Task<IActionResult> SendVerificationCode(
        [FromServices] SendVerificationCodeHandler handler,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserIdRequired();
        
        var result = await handler.Handle(userId, cancellationToken);
        
        if (result.IsFailure)
            return result.Error.ToResponse();
        
        return Ok(Envelope.Ok(null));
    }

    [Authorize]
    [HttpGet("say-hello")]
    public async Task<IActionResult> SayHello()
    {
        return Ok(Envelope.Ok("Hello"));
    }
}