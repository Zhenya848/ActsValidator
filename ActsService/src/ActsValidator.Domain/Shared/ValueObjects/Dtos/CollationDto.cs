namespace ActsValidator.Domain.Shared.ValueObjects.Dtos;

public record CollationDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }

    public string Act1Name { get; init; }
    public string Act2Name { get; init; }

    public int CoincidencesCount { get; init; }
    public int RowsProcessed { get; init; }
    public DiscrepancyDto[] CollationErrors { get; init; } = [];
    
    public string Status { get; init; }
    public DateTime CreatedAt { get; init; }

    private CollationDto() { }
    
    public CollationDto(
        Guid userId, 
        Guid id, 
        string act1Name, 
        string act2Name, 
        int coincidencesCount,
        int rowsProcessed,
        DiscrepancyDto[] errors,
        string status,
        DateTime createdAt)
    {
        Id = id;
        UserId = userId;
        Act1Name = act1Name;
        Act2Name = act2Name;
        CoincidencesCount = coincidencesCount;
        RowsProcessed = rowsProcessed;
        CollationErrors = errors;
        Status = status;
        CreatedAt = createdAt;
    }
}