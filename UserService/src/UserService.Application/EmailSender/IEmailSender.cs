using CSharpFunctionalExtensions;
using UserService.Domain.Shared;
using UserService.Domain.Shared.ValueObjects.EmailSender;

namespace UserService.Application.EmailSender;

public interface IEmailSender
{
    Task<UnitResult<ErrorList>> Send(MailData mailData);
}