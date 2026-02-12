using CSharpFunctionalExtensions;
using Microsoft.Extensions.Options;
using MimeKit;
using UserService.Application.Abstractions;
using UserService.Application.Models;
using UserService.Domain.Shared;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace UserService.Infrastructure;

public class EmailSender : IEmailSender
{
    private readonly MailOptions _mailOptions;

    public EmailSender(IOptions<MailOptions> mailOptions)
    {
        _mailOptions = mailOptions.Value;
    }
    
    public async Task<UnitResult<ErrorList>> Send(MailData mailData)
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
}