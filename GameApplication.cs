namespace Markwardt;

public class GameApplication : IServiceApplication
{
    public required Factory<IBotClient> ClientFactory { get; init; }

    public async ValueTask Run()
    {
        using IBotClient client = ClientFactory();
        await client.Start(await File.ReadAllTextAsync("Token.txt"));
        await Task.Delay(-1);
    }
}