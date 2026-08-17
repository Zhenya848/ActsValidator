using ChatService.Abstractions;
using ChatService.Models.Shared;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Options;
using MimeKit;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace ChatService.EmailSender;

public class EmailSender : IEmailSender
{
    private readonly MailOptions _mailOptions;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(IOptions<MailOptions> mailOptions, ILogger<EmailSender> logger)
    {
        _mailOptions = mailOptions.Value;
        _logger = logger;
    }
    
    private async Task Send(MailData mailData)
    {
        try
        {
            var mail = new MimeMessage();

            mail.From.Add(new MailboxAddress(_mailOptions.FromDisplayName, _mailOptions.From));

            var tryParse = MailboxAddress.TryParse(mailData.To, out var to);

            if (tryParse == false)
                throw new FormatException("Invalid mail address.");

            mail.To.Add(to);

            var body = new BodyBuilder
            {
                HtmlBody = mailData.Body
            };

            mail.Body = body.ToMessageBody();
            mail.Subject = mailData.Subject;

            using var client = new SmtpClient();

            await client.ConnectAsync(_mailOptions.Host, _mailOptions.Port);
            await client.AuthenticateAsync(_mailOptions.UserName, _mailOptions.Password);
            await client.SendAsync(mail);
        }
        catch (Exception ex)
        {
            _logger.LogError("{message}", ex.Message);
        }
    }

    public async Task SendMessageNotificationToSupports(
        Guid chatId, 
        string email, 
        string messageContent)
    {
        var mailData = new MailData(
            email, 
            "Новое уведомление!", 
            $"Сообщение в поддержку: {messageContent}. Вы можете ответить на него по ссылке: " +
                $"https://my_site.ru/api/Chats/chat?chatId={chatId}");

        await Send(mailData);
    }
}