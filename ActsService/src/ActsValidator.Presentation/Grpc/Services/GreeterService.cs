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
            return (ErrorList)Error.Failure("subtract.token.failure", ex.Status.Detail);
        }
    }
}