namespace AiMessaging.Contracts.Messaging;

public record SendToAiCommand(Guid AiRequestId, string Prompt);