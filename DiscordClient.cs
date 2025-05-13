using NetCord.Rest;

namespace Markwardt;

public interface IDiscordClient : IDisposable
{
    ValueTask Start(string token);
    ValueTask<RestMessage> Send(ulong channel, MessageProperties properties);
}

public class DiscordClient : BaseDisposable, IDiscordClient
{
    private GatewayClient? client;

    public required IMessageHandler Handler { get; init; }

    public async ValueTask<RestMessage> Send(ulong channel, MessageProperties properties)
        => await client!.Rest.SendMessageAsync(channel, properties);

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