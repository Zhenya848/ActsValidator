namespace UserService.Domain.Shared;

public abstract class Entity<TId>
{
    public TId Id { get; private set; }

    protected Entity(TId id) => Id = id;
}