using ActsValidator.Domain.Shared;
using ActsValidator.Domain.Shared.ValueObjects.Ids;
using ActsValidator.Domain.ValueObjects;
using CSharpFunctionalExtensions;

namespace ActsValidator.Domain;

public class AiRequest : Shared.Entity<AiRequestId>
{
    public CollationId CollationId { get; private set; }

    public AiRequestStatus Status { get; private set; } = AiRequestStatus.Pending;
    public List<Discrepancy> AiDiscrepancies { get; private set; } = [];
    public string? ErrorMessage { get; private set; }

    private AiRequest(AiRequestId id) : base(id)
    {
        
    }
    
    public AiRequest(AiRequestId id, CollationId collationId) : base(id)
    {
        CollationId = collationId;
    }

    public void Complete(List<Discrepancy> aiDiscrepancies)
    {
        AiDiscrepancies = aiDiscrepancies;
        Status = AiRequestStatus.Completed;
    }

    public UnitResult<Error> SetErrorMessage(string? message)
    {
        if (message is not null && string.IsNullOrWhiteSpace(message))
            return Errors.General.ValueIsInvalid(nameof(message));
        
        ErrorMessage = message;
        Status = AiRequestStatus.Failed;

        return Result.Success<Error>();
    }
}