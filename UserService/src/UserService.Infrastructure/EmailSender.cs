using System.Data;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using UserService.Application.Abstractions;
using UserService.Application.Commands.RegisterUser;
using UserService.Application.Models;
using UserService.Domain.Shared;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace UserService.Infrastructure;

public class EmailSender : IEmailSender
{
    private readonly MailOptions _mailOptions;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(IOptions<MailOptions> mailOptions, ILogger<EmailSender> logger)
    {
        _mailOptions = mailOptions.Value;
        _logger = logger;
    }
    
    public async Task<UnitResult<ErrorList>> Send(MailData mailData)
    {
        try
        {
            var mail = new MimeMessage();

            mail.From.Add(new MailboxAddress(_mailOptions.FromDisplayName, _mailOptions.From));

            var tryParse = MailboxAddress.TryParse(mailData.To, out var to);

            if (tryParse == false)
                return (ErrorList)Errors.General.Failure("Email");

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

            return UnitResult.Success<ErrorList>();
        }
        catch (Exception ex)
        {
            return (ErrorList)Error.Failure("send.email.failure", ex.Message);
        }
    }
}