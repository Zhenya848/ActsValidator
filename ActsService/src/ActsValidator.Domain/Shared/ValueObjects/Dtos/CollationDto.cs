namespace ActsValidator.Domain.Shared.ValueObjects.Dtos;

public record CollationDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }

    public string Act1Name { get; init; }
    public string Act2Name { get; init; }

    public DiscrepancyDto[] Discrepancies { get; init; } = [];
    public AiRequestDto? AiRequest { get; init; }

    private CollationDto() { }
    
    public CollationDto(
        Guid userId, 
        Guid id, 
        string act1Name, 
        string act2Name, 
        DiscrepancyDto[] discrepancies,  
        AiRequestDto? aiRequest = null)
    {
        Id = id;
        UserId = userId;
        Act1Name = act1Name;
        Act2Name = act2Name;
        Discrepancies = discrepancies;
        AiRequest = aiRequest;
    }
}