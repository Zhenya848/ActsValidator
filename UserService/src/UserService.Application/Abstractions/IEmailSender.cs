using CSharpFunctionalExtensions;
using UserService.Application.Models;
using UserService.Domain.Shared;

namespace UserService.Application.Abstractions;

public interface IEmailSender
{
    Task<UnitResult<ErrorList>> Send(MailData mailData);
}