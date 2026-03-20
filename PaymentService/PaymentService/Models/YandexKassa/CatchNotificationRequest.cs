namespace PaymentService.Models.YandexKassa;

public record CatchNotificationRequest
{
    public string Type;
    public string Event;
    public PaymentObject Object;
}