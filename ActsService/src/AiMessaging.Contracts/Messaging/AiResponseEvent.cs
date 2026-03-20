namespace AiMessaging.Contracts.Messaging;

public record AiResponseEvent(Guid AiRequestId, string? Response, string? Error = null);