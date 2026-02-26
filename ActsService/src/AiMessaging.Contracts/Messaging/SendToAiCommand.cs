namespace AiMessaging.Contracts.Messaging;

public record SendToAiCommand(string Prompt, Guid AiRequestId);