namespace PaymentService.Models.YandexKassa;

public record PaymentObject(
    string Id,
    string Status,
    Amount Amount,
    Amount IncomeAmount,
    string Description,
    Recipient Recipient,
    PaymentMethod PaymentMethod,
    DateTimeOffset CapturedAt,
    DateTimeOffset CreatedAt,
    bool Test,
    Amount RefundedAmount,
    bool Paid,
    bool Refundable,
    Dictionary<string, string> Metadata);