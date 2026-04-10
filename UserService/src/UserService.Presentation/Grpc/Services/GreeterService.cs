using Grpc.Core;
using UserService.Application.Commands.MakeAction;
using UserService.Domain;

namespace UserService.Presentation.Grpc.Services;

public class GreeterService : Greeter.GreeterBase
{
    private readonly MakeActionHandler _handler;

    public GreeterService(MakeActionHandler handler)
    {
        _handler = handler;
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

        var result = await _handler
            .Handle(userId, context.CancellationToken);
        
        if (result.IsFailure)
            throw new RpcException(new Status(
                StatusCode.FailedPrecondition, 
                string.Join(", ", result.Error.Select(e => $"{e.Code}: {e.Message}"))
            ));

        return new MakeActionResponse();
    }
}