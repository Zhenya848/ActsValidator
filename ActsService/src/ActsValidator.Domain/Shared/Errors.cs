namespace ActsValidator.Domain.Shared;

public class Errors
{
    public class General
    {
        public static Error ValueIsInvalid(string? name = null) =>
            Error.Validation("value.is.invalid", $"{(name != null ? name : "value")} is invalid", name ?? null);

        public static Error NotFound(Guid? id = null) =>
            Error.NotFound("record.not.found", $"record not found{(id != null ? " for id: " + id : "")}", 
                id is not null ? id.ToString() : null);

        public static Error Failure(string? name = null) =>
            Error.Failure("failure", $"{(name != null ? name : "value")} is failure");

        public static Error ValueIsRequired(string? name = null) =>
            Error.Validation("value.is.required", $"{(name != null ? name : "value")} is required", name ?? null);
    }
    
    public static class User
    {
        public static Error AlreadyExist() =>
            Error.Validation("user.already.exist", "user with this name already exist!");
        
        public static Error NotFound() => 
            Error.NotFound("user.not.found", "user not found");
        
        public static Error NotFound(string email) => 
            Error.NotFound("user.not.found", $"user with email {email} not found");
            
        public static Error WrongCredentials() =>
            Error.Validation("user.wrong.credentials", "user with wrong credentials");
        
        public static Error NotVerified() =>
            Error.Validation("user.not.verified", "user email not verified");
        
        public static Error InvalidBalance() =>
            Error.Validation("user.invalid.balance", "user with invalid balance");
    }
    
    public static class Token
    {
        public static Error InvalidToken()
        {
            return Error.Validation("token.is.invalid", "Your token is invalid");
        }
    }
}