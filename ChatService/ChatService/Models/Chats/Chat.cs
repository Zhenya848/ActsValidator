using ChatService.Models.Shared;
using ChatService.Models.Shared.ValueObjects.Id;
using ChatService.Models.ValueObjects;
using CSharpFunctionalExtensions;

namespace ChatService.Models.Chats;

public class Chat : Shared.Entity<ChatId>
{
    public UserData  User { get; }
    private List<Message> _messages = [];
    public IReadOnlyList<Message> Messages => _messages;

    private Chat(ChatId id) : base(id)
    {
        
    }
    
    public Chat(UserData user, ChatId id) : base(id)
    {
        User = user;
    }

    public void AddMessage(Message message) => _messages.Add(message);
    public void RemoveMessage(Message message) => _messages.Remove(message);
}