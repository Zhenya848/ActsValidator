using ChatService.Models.Shared;
using CSharpFunctionalExtensions;

namespace ChatService.Abstractions;

public interface IEmailSender
{
    Task SendMessageNotificationToSupports(Guid chatId, string email, string messageContent);
}