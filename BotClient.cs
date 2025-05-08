namespace Markwardt;

public interface IBotClient : IDisposable
{
    ValueTask Start(string token);
}

public class BotClient : BaseDisposable, IBotClient
{
    private GatewayClient? client;

    public required IMessageHandler Handler { get; init; }

    public async ValueTask Start(string token)
    {
        client ??= new(new BotToken(token));
        client.MessageCreate += Handler.Handle;
        await client.StartAsync();
    }

    protected override void OnDispose()
    {
        base.OnDispose();

        client?.Dispose();
    }
}