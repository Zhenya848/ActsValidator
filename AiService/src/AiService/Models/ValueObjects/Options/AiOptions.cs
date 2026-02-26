namespace AiService.Models.ValueObjects.Options;

public record AiOptions
{
    public const string Ai = "AiOptions";
    public string ApiKey { get; init; }
    public string Model { get; init; }
}