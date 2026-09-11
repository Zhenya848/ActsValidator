using System.Net.Mail;
using ChatService.Models.Shared;
using CSharpFunctionalExtensions;

namespace ChatService.Models.ValueObjects;

public record UserData
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string Email { get; init; }

    private UserData(Guid id, string name, string email)
    {
        Id = id;
        Name = name;
        Email = email;
    }

    public static Result<UserData, Error> Create(Guid id, string name, string email)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Errors.General.ValueIsRequired("name");
        
        if (MailAddress.TryCreate(email, out _) == false)
            return Errors.General.ValueIsInvalid("email");
        
        return new UserData(id, name, email);
    }
}