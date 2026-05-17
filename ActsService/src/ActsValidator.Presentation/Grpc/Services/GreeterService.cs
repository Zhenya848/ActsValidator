using ActsValidator.Application.Abstractions;
using ActsValidator.Domain.Shared;
using CSharpFunctionalExtensions;
using Grpc.Core;

namespace ActsValidator.Presentation.Grpc.Services;

public class GreeterService : IGreeterService
{
    private readonly Greeter.GreeterClient _client;

    public GreeterService(Greeter.GreeterClient client)
    {
        _client = client;
    }

    public async Task<UnitResult<ErrorList>> MakeAction(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new MakeActionRequest() 
                { UserId = userId.ToString() };

            await _client.MakeActionAsync(request, cancellationToken: cancellationToken);

            return Result.Success<ErrorList>();
        }
        catch (RpcException ex)
        {
            var splitMessage = ex.Status.Detail.Split(": ");
            
            if (splitMessage.Length > 1)
                return (ErrorList)Error.Failure(splitMessage[0], splitMessage[1]);
            
            return (ErrorList)Error.Failure("subtract.token.failure", ex.Status.Detail);
        }
    }
}