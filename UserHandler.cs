namespace Markwardt;

public interface IUserController
{
    void SetHandler(User user, IMessageHandler? handler);
}

[DefaultImplementation<DefaultHandler>]
public interface IMessageHandler
{
    ValueTask Handle(Message message);
}

public class UserController : IUserController, IMessageHandler
{
    private readonly Dictionary<ulong, IMessageHandler> handlers = [];

    public required DefaultHandler DefaultHandler { get; init; }

    public void SetHandler(User user, IMessageHandler? handler)
    {
        if (handler is null)
        {
            handlers.Remove(user.Id);
        }
        else
        {
            handlers[user.Id] = handler;
        }
    }

    public async ValueTask Handle(Message message)
        => await (handlers.GetValueOrDefault(message.Author.Id) ?? DefaultHandler).Handle(message);
}

public class DefaultHandler : IMessageHandler
{
    public ValueTask Handle(Message message)
    {
        throw new NotImplementedException();
    }
}
