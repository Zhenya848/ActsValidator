namespace PaymentService.Models.YandexKassa;

public record Recipient(
    string AccountId,
    string GatewayId);