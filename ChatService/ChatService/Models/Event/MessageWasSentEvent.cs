using ChatService.Models.ValueObjects;

namespace ChatService.Models.Event;

public record MessageWasSentEvent(
    string MessageContent,
    Guid ChatId,
    Guid SenderId,
    DateTime CreatedAt);