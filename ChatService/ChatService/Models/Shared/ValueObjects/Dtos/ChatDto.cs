using ChatService.Models.Shared.ValueObjects.Id;
using ChatService.Models.ValueObjects;
using CSharpFunctionalExtensions;

namespace ChatService.Models.Shared.ValueObjects.Dtos;

public class ChatDto
{
    public Guid Id { get; init; }
    public UserData User { get; init; }
    public MessageDto[] Messages { get; init; }

    public ChatDto(ChatId id, UserData user, MessageDto[] messages)
    {
        Id = id;
        User = user;
        Messages = messages;
    }
}