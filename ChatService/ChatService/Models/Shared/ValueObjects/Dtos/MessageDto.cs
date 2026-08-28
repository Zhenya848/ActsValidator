using ChatService.Models.Shared.ValueObjects.Id;
using CSharpFunctionalExtensions;

namespace ChatService.Models.Shared.ValueObjects.Dtos;

public record MessageDto
{
    public Guid ChatId { get; init; }
    public string SendType { get; init; }
    public string Content { get; init; }
    public bool IsRedacted { get; init; }
    public DateTime CreatedAt { get; init; }

    public static Result<MessageDto, ErrorList> Create(
        ChatId chatId, 
        string sendType,
        string content, 
        bool isRedacted,
        DateTime createdAt)
    {
        var errors = new List<Error>();
        
        if (string.IsNullOrWhiteSpace(sendType))
            errors.Add(Errors.General.ValueIsRequired("send type"));
        
        if (string.IsNullOrWhiteSpace(content))
            errors.Add(Errors.General.ValueIsRequired("content"));
        
        if (createdAt > DateTime.UtcNow)
            errors.Add(Errors.General.ValueIsInvalid("created at"));
        
        if (errors.Count > 0)
            return (ErrorList)errors;

        return new MessageDto()
        {
            ChatId = chatId,
            SendType = sendType,
            Content = content,
            IsRedacted = isRedacted,
            CreatedAt = createdAt
        };
    }
}