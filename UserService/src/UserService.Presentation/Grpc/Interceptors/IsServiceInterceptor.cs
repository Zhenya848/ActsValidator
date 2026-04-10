using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using UserService.Presentation.Options;

namespace UserService.Presentation.Grpc.Interceptors;

public class IsServiceInterceptor : Interceptor
{
    private readonly AuthOptions _authOptions;

    public IsServiceInterceptor(IOptions<AuthOptions> authOptions)
    {
        _authOptions = authOptions.Value;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var secretKey = context.RequestHeaders.FirstOrDefault(
            h => h.Key == "X-Internal-Service-Key".ToLower())?.Value;

        if (secretKey is null)
        {
            throw new RpcException(new Status(
                StatusCode.Unauthenticated,
                "Secret key was not found"
            ));
        }
        
        if (secretKey != _authOptions.SecretKey)
        {
            throw new RpcException(new Status(
                StatusCode.Unauthenticated,
                "Invalid secret key"
            ));
        }

        return await continuation(request, context);
    }
}