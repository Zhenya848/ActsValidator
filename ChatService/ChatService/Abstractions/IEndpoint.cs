namespace ChatService.Abstractions;

public interface IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app);
}