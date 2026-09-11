using CSharpFunctionalExtensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using PaymentService.Models.Shared;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace PaymentService.EmailSender;

public class EmailSender
{
    private readonly MailOptions _mailOptions;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(IOptions<MailOptions> mailOptions, ILogger<EmailSender> logger)
    {
        _mailOptions = mailOptions.Value;
        _logger = logger;
    }

    public async Task<UnitResult<ErrorList>> SendCheck(
        decimal amount, 
        string email, 
        string currency, 
        string productName, 
        int quantity = 1)
    {
        
        
        return Result.Success<ErrorList>();
    }
    
    private async Task<UnitResult<ErrorList>> Send(MailData mailData)
    {
        try
        {
            var mail = new MimeMessage();

            mail.From.Add(new MailboxAddress(_mailOptions.FromDisplayName, _mailOptions.From));

            var tryParse = MailboxAddress.TryParse(mailData.To, out var to);

            if (tryParse == false)
                return (ErrorList)Errors.General.ValueIsInvalid("Email");

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