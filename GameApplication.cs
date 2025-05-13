namespace Markwardt;

public class GameApplication : IServiceApplication
{
    public required Factory<IDiscordClient> ClientFactory { get; init; }

    public async ValueTask Run()
    {
        using IDiscordClient client = ClientFactory();
        await client.Start(await File.ReadAllTextAsync("Token.txt"));
        await Task.Delay(-1);
    }
}