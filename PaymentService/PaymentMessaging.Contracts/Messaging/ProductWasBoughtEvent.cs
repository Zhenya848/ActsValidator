namespace PaymentMessaging.Contracts.Messaging;

public record ProductWasBoughtEvent(Guid Id, Guid UserId, string ProductId);