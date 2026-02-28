using ActsValidator.Domain.ValueObjects;

namespace ActsValidator.Domain.Shared.ValueObjects.Dtos;

public record AiRequestDto
{
    public Guid Id { get; init; }
    public Guid CollationId { get; init; }
    public AiRequestStatus Status { get; init; }
    public DiscrepancyDto[] Discrepancies { get; init; } = [];
    public string? ErrorMessage { get; init; }
    
    private AiRequestDto() { }
    
    public AiRequestDto(Guid id, Guid collationId, AiRequestStatus status, string? errorMessage)
    {
        Id = id;
        CollationId = collationId;
        Status = status;
        ErrorMessage = errorMessage;
    }
}