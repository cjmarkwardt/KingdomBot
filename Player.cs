
using System.Threading.Tasks;

namespace Markwardt;

public record World
{
    public required string Owner { get; init; }
    public required string Name { get; init; }
}

public interface IChannel
{
    ValueTask Send(string? player, string message);
    ValueTask<bool> Query(string? player, string message);
    ValueTask<string> Select(string? player, string message, IEnumerable<string> options);
    ValueTask<string> SelectPlayer(string? player, string message);
}

public interface IWorldGenerator
{
    World Generate(string owner, string name);
}

public interface ICampaignManager
{
    IEnumerable<string> List(string owner);
    ValueTask Delete(string owner, string world);
    ValueTask Create(string owner, IChannel channel, string world);
    ValueTask Host(string owner, IChannel channel, string world);
    ValueTask Join(string player, string owner);
    ValueTask Leave(string player);
}

public interface IWorldStore
{
    IEnumerable<string> List(string owner);
    ValueTask<World> Load(string owner, string world);
    ValueTask Save(World world);
    ValueTask Delete(World world);
}

[ServiceType<string>]
public class WorldFolderPathTag : ServiceTag
{
    protected override object GetService(IServiceProvider services)
        => Path.Combine(Environment.CurrentDirectory, "Worlds");
}

public interface IWorldSerializer
{
    string DataType { get; }

    string Serialize(World world);
    World Deserialize(string text);
}

public class WorldStore([Inject<WorldFolderPathTag>] string folder) : IWorldStore
{
    private string FileExtension => "." + Serializer.DataType;

    public required IWorldSerializer Serializer { get; init; }

    public IEnumerable<string> List(string owner)
        => Directory.EnumerateFiles(GetOwnerFolder(owner)).Select(x => x[..^FileExtension.Length]);

    public async ValueTask Save(World world)
    {
        string file = GetWorldFile(world.Owner, world.Name);
        Directory.CreateDirectory(GetOwnerFolder(world.Owner));
        await File.WriteAllTextAsync(file, Serializer.Serialize(world));
    }

    public async ValueTask<World> Load(string owner, string world)
        => Serializer.Deserialize(await File.ReadAllTextAsync(GetWorldFile(owner, world)));

    public ValueTask Delete(World world)
    {
        File.Delete(GetWorldFile(world.Owner, world.Name));
        return ValueTask.CompletedTask;
    }

    private string GetOwnerFolder(string owner)
        => Path.Combine(folder, owner);

    private string GetWorldFile(string owner, string world)
        => Path.Combine(folder, owner, FileExtension);
}

public class CampaignManager : ICampaignManager
{
    private readonly Dictionary<string, ICampaignController> playerCampaigns = [];

    public required ICampaignController.Factory CampaignFactory { get; init; }
    public required IWorldGenerator Generator { get; init; }
    public required IWorldStore Store { get; init; }

    public IEnumerable<string> List(string owner)
        => Store.List(owner);

    public async ValueTask Join(string player, string owner)
    {
        await Leave(player);

        if (playerCampaigns.TryGetValue(owner, out ICampaignController? campaign))
        {
            playerCampaigns.Add(player, campaign);
            await campaign.Join(player);
        }
    }

    public async ValueTask Leave(string player)
    {
        if (playerCampaigns.TryGetValue(player, out ICampaignController? campaign))
        {
            playerCampaigns.Remove(player);
            await campaign.Leave(player);
        }
    }

    public async ValueTask Delete(string owner, string world)
    {
        if (playerCampaigns.TryGetValue(owner, out ICampaignController? campaign) && campaign.Owner.Id == owner)
        {
            playerCampaigns.Remove(owner);
            await campaign.Delete();
        }
    }

    public async ValueTask Create(string owner, IChannel channel, string world)
        => await Host(owner, channel, Generator.Generate(owner, world));

    public async ValueTask Host(string owner, IChannel channel, string world)
        => await Host(owner, channel, await Store.Load(owner, world));

    private async ValueTask Host(string owner, IChannel channel, World world)
    {
        await Leave(owner);
        playerCampaigns.Add(owner, CampaignFactory(channel, owner, world));
    }
}

public interface ICampaignRunner
{
    delegate ICampaignRunner Factory(ICampaignController campaign);

    ValueTask Start();
    void Stop();
}

public interface ICampaign
{
    World World { get; }
    IChannel Channel { get; }
    string Owner { get; }
    IReadOnlyDictionary<string, string> Players { get; }
}

public interface ICampaignController : ICampaign
{
    delegate ICampaignController Factory(IChannel channel, string owner, World world);

    ValueTask Receive(string player, )
    ValueTask Save();
    ValueTask Join(string player);
    ValueTask Leave(string player);
    ValueTask Start();
    ValueTask Delete();
}

public class Campaign : ICampaignController
{
    public Campaign(ICampaignRunner.Factory runnerFactory, IChannel channel, string owner, World world)
    {
        this.channel = channel;
        this.world = world;

        Owner = owner.Id;
        players.Add(Owner, owner);

        runner = runnerFactory(this);
    }

    private readonly IChannel channel;
    private readonly World world;
    private readonly ICampaignRunner runner;
    private readonly Dictionary<string, string> players = [];

    public string Owner { get; }
    public IReadOnlyDictionary<string, string> Players => players;

    public required IWorldStore Store { get; init; }

    public async ValueTask Save()
        => await Store.Save(world);

    public async ValueTask Join(string player)
    {
        if (players.TryAdd(player.Id, player))
        {

        }
    }

    public async ValueTask Leave(string player)
    {
        if (players.Remove(player))
        {
            if (player == Owner)
            {

                await Save();
            }
        }
    }

    public async ValueTask Start()
    {

    }

    public async ValueTask Delete()
    {
        await Store.Delete(world);
    }
}