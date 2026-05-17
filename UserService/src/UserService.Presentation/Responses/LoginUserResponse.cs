using UserService.Domain.Shared;

namespace UserService.Presentation.Responses;

public record LoginUserResponse(string AccessToken, UserInfo User);