using CSharpFunctionalExtensions;
using PaymentService.Models;
using PaymentService.Models.Shared;

namespace PaymentService.Abstractions;

public interface IEmailSender
{
    Task SendReceiptToEmail(
        Receipt receipt,
        CancellationToken cancellationToken = default);
}