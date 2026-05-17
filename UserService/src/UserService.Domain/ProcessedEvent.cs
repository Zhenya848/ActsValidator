using CSharpFunctionalExtensions;
using UserService.Domain.Shared;

namespace UserService.Domain;

public class ProcessedEvent : Shared.Entity<Guid>
{
    public DateTime CreatedAt { get; init; }
    
    private ProcessedEvent(Guid id) : base(id)
    {
        
    }

    private ProcessedEvent(Guid id, DateTime createdAt) : base(id)
    {
        CreatedAt = createdAt;
    }

    public static Result<ProcessedEvent, ErrorList> Create(Guid id, DateTime createdAt)
    {
        if (id.Equals(Guid.Empty))
            return (ErrorList)Errors.General.ValueIsInvalid(nameof(id));
        
        if (createdAt > DateTime.Now)
            return (ErrorList)Errors.General.ValueIsInvalid(nameof(createdAt));
        
        return new ProcessedEvent(id, createdAt);
    }
}