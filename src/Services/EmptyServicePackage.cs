namespace Markwardt;

public class EmptyServicePackage : IServicePackage
{
    public static EmptyServicePackage Instance { get; } = new();

    private EmptyServicePackage() { }

    public void Configure(IServiceConfiguration configuration) { }
}