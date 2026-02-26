namespace ActsValidator.Application.Collations.Commands.Create;

public record CreateCollationCommand(Guid UserId, string Act1Name, string Act2Name, Stream Stream1, Stream Stream2);