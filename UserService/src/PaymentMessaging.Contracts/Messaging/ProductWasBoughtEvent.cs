namespace PaymentMessaging.Contracts.Messaging;

public record ProductWasBoughtEvent(Guid UserId, string ProductId);