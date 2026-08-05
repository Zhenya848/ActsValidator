using ChatService.Models.Shared;
using ChatService.Models.Shared.ValueObjects.Id;
using ChatService.Models.ValueObjects;
using CSharpFunctionalExtensions;

namespace ChatService.Models.Chats;

public class Message : Shared.Entity<MessageId>
{
    public ChatId ChatId { get; }
    public int SerialNumber { get; init; }
    public MessageRole Role { get; }
    public string Content { get; private set; }
    public bool IsRedacted { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime CreatedAt { get; }
    
    private Message(MessageId id) : base(id)
    {
        
    }

    private Message(
        MessageId id,
        ChatId chatId,
        int serialNumber,
        MessageRole role,
        string content,
        DateTime createdAt) : base(id)
    {
        ChatId = chatId;
        SerialNumber = serialNumber;
        Role = role;
        Content = content;
        CreatedAt = createdAt;
    }

    public static Result<Message, ErrorList> Create(
        ChatId chatId, 
        int serialNumber, 
        MessageRole role, 
        string content)
    {
        if (serialNumber < 1)
            return (ErrorList)Errors.General.ValueIsInvalid("serial number");
        
        var validateContentResult = ValidateContent(content);

        if (validateContentResult.IsFailure)
            return validateContentResult.Error;

        return new Message(MessageId.AddNewId(), chatId, serialNumber, role, content, DateTime.UtcNow);
    }
    
    public UnitResult<ErrorList> Update(string newContent)
    {
        var validateContentResult = ValidateContent(newContent);

        if (validateContentResult.IsFailure)
            return validateContentResult.Error;
        
        Content = newContent;
        IsRedacted = true;

        return Result.Success<ErrorList>();
    }

    private static UnitResult<ErrorList> ValidateContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return (ErrorList)Errors.General.ValueIsRequired("content");

        if (content.Length > Constants.MAX_MESSAGE_LENGTH)
            return (ErrorList)Error.Validation(
                "content.too.long", 
                $"Content is too long! Max length is {Constants.MAX_MESSAGE_LENGTH} characters.");
        
        return Result.Success<ErrorList>();
    }
    
    public void Delete() => IsDeleted = true;
}