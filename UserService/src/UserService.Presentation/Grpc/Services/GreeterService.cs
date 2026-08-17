using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using UserService.Application.Commands.MakeAction;
using UserService.Application.Queries.GetUsersByPermissions;
using UserService.Domain;

namespace UserService.Presentation.Grpc.Services;

public class GreeterService : Greeter.GreeterBase
{
    private readonly MakeActionHandler _makeActionHandler;
    private readonly GetUsersByPermissionsHandler _getUsersByPermissionsHandler;

    public GreeterService(
        MakeActionHandler makeActionHandler, 
        GetUsersByPermissionsHandler getUsersByPermissionsHandler)
    {
        _makeActionHandler = makeActionHandler;
        _getUsersByPermissionsHandler = getUsersByPermissionsHandler;
    }
    
    public override async Task<MakeActionResponse> MakeAction(
        MakeActionRequest request, 
        ServerCallContext context)
    {
        if (Guid.TryParse(request.UserId, out var userId) == false)
            throw new RpcException(new Status(
                StatusCode.InvalidArgument, 
                "Invalid user id"
            ));

        var result = await _makeActionHandler
            .Handle(userId, context.CancellationToken);
        
        if (result.IsFailure)
            throw new RpcException(new Status(
                StatusCode.FailedPrecondition, 
                $"{result.Error.Code}: {result.Error.Message}"
            ));

        return new MakeActionResponse();
    }

    public override async Task<GetUsersByPermissionsResponse> GetUsersByPermissions(
        GetUsersByPermissionsRequest request, 
        ServerCallContext context)
    {
        var result = await _getUsersByPermissionsHandler
            .Handle(request.Permissions.ToArray(), context.CancellationToken);

        return new GetUsersByPermissionsResponse() { UserEmails = { result } };
    }
}