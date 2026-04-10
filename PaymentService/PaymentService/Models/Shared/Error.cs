namespace PaymentService.Models.Shared;

public class Error
{
    public string Code { get; }
    public string Message { get; }
    public ErrorType ErrorType { get; }
    public string? InvalidField { get; } = null;

    private Error(string code, string message, ErrorType errorType, string? invalidField = null)
    {
        Code = code;
        Message = message;
        ErrorType = errorType;
        InvalidField = invalidField;
    }
    
    public static Error Validation(string code, string message, string? invalidField = null) =>
        new Error(code, message, ErrorType.Validation, invalidField);

    public static Error NotFound(string code, string message) =>
        new Error(code, message, ErrorType.NotFound);

    public static Error Failure(string code, string message) =>
        new Error(code, message, ErrorType.Failure);

    public static Error Conflict(string code, string message) =>
        new Error(code, message, ErrorType.Conflict);
}