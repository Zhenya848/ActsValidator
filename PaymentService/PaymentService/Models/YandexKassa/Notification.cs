namespace PaymentService.Models.YandexKassa;

public record Notification(
    string Type,
    string Event,
    PaymentObject Object);