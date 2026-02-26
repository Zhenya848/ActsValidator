namespace AiMessaging.Contracts.Messaging;

public record AiResponseEvent(string Response, Guid AiRequestId);