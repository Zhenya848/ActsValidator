using System.Globalization;
using System.Net;
using CSharpFunctionalExtensions;
using MailKit.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using PaymentService.Abstractions;
using PaymentService.Models;
using PaymentService.Models.Shared;
using Quartz.Xml;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace PaymentService.EmailSender;

public class EmailSender : IEmailSender
{
    private readonly MailOptions _mailOptions;

    public EmailSender(IOptions<MailOptions> mailOptions)
    {
        _mailOptions = mailOptions.Value;
    }

    public async Task SendReceiptToEmail(
        Receipt receipt,
        CancellationToken cancellationToken = default)
    {
        var amountText = receipt.Amount.ToString(
            "N2",
            CultureInfo.GetCultureInfo("ru-RU"));

        var dateText = receipt.OccurredOn.ToString(
            "dd.MM.yyyy",
            CultureInfo.InvariantCulture);

        var encodedUrl = WebUtility.HtmlEncode(receipt.PrintUrl?.AbsoluteUri 
            ?? throw new Exception("Не удалось получить URL чека"));

        var message = new MimeMessage();

        message.From.Add(new MailboxAddress(
            _mailOptions.FromDisplayName,
            _mailOptions.From));

        message.To.Add(MailboxAddress.Parse(receipt.CustomerEmail));
        message.Subject = "Подтверждение оплаты";

        var bodyBuilder = new BodyBuilder();

        bodyBuilder.TextBody = $"""
            Подтверждение оплаты

            Сумма: {amountText} ₽
            Дата: {dateText}

            Чек:
            {receipt.PrintUrl.AbsoluteUri}
            """;

        bodyBuilder.HtmlBody = $"""
            <!DOCTYPE html>
            <html lang="ru">
            <body>
                <h2>Подтверждение оплаты</h2>

                <p>Оплата успешно проведена.</p>

                <p>
                    <strong>Сумма:</strong> {amountText} ₽<br>
                    <strong>Дата:</strong> {dateText}
                </p>

                <p>
                    <a href="{encodedUrl}"
                       style="
                           display:inline-block;
                           padding:12px 20px;
                           background:#2563eb;
                           color:#fff;
                           text-decoration:none;
                           border-radius:6px;">
                        Открыть чек
                    </a>
                </p>

                <p>
                    <small>
                        Если кнопка не работает, воспользуйтесь ссылкой:<br>
                        {encodedUrl}
                    </small>
                </p>
            </body>
            </html>
            """;

        message.Body = bodyBuilder.ToMessageBody();

        try
        {
            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(
                _mailOptions.Host,
                _mailOptions.Port, 
                SecureSocketOptions.Auto, 
                cancellationToken);

            await smtp.AuthenticateAsync(
                _mailOptions.UserName,
                _mailOptions.Password,
                cancellationToken);

            await smtp.SendAsync(message, cancellationToken);
            await smtp.DisconnectAsync(true, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new Exception($"Не удалось отправить чек: {ex.Message}");
        }
    }
}