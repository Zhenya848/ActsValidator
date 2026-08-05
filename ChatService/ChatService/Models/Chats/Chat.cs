using ChatService.Models.Shared;
using ChatService.Models.Shared.ValueObjects.Id;
using CSharpFunctionalExtensions;

namespace ChatService.Models.Chats;

public class Chat : Shared.Entity<ChatId>
{
    public Guid UserId { get; }
    private List<Message> _messages = [];
    public IReadOnlyList<Message> Messages => _messages;

    private Chat(ChatId id) : base(id)
    {
        
    }
    
    private Chat(Guid userId, ChatId id) : base(id)
    {
        UserId = userId;
    }

    public static Result<Chat, ErrorList> Create(Guid userId)
    {
        if (userId.Equals(Guid.Empty))
            return (ErrorList)Errors.General.ValueIsRequired("user id");

        return new Chat(userId, ChatId.AddNewId());
    }
    
    public void AddMessage(Message message) => _messages.Add(message);
    public void RemoveMessage(Message message) => _messages.Remove(message);
}