using ActsValidator.Presentation.Options;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Options;

namespace ActsValidator.Presentation.Grpc.Interceptors;

public class ProvideSecretKeyInterceptor : Interceptor
{
    private readonly AuthOptions _authOptions;

    public ProvideSecretKeyInterceptor(IOptions<AuthOptions> authOptions)
    {
        _authOptions = authOptions.Value;
    }

    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        var headers = new Metadata
        {
            { "X-Internal-Service-Key", _authOptions.SecretKey }
        };

        var newContext = new ClientInterceptorContext<TRequest, TResponse>(
            context.Method,
            context.Host,
            context.Options.WithHeaders(headers)
        );

        return continuation(request, newContext);
    }
}