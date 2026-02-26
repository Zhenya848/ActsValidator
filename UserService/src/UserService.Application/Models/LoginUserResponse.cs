using UserService.Domain.Shared;

namespace UserService.Application.Models;

public record LoginUserResponse(string AccessToken, Guid RefreshToken, UserInfo User);