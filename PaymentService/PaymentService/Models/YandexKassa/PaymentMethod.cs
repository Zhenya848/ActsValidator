namespace PaymentService.Models.YandexKassa;

public record PaymentMethod(
    string Type,
    string Id,
    bool Saved,
    string Status,
    string Title,
    string AccountNumber);