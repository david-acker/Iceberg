namespace Iceberg.CommandLine.Commands.Load;

internal interface ILoadCommandHandler
{
    Task Handle(IEnumerable<string> paths);
}

internal sealed class LoadCommandHandler : ILoadCommandHandler
{
    private readonly IIcebergSessionContext _sessionContext;

    public LoadCommandHandler(IIcebergSessionContext sessionContext)
    {
        _sessionContext = sessionContext;
    }

    public async Task Handle(IEnumerable<string> paths)
    {
        if (paths.Count() == 1)
            await _sessionContext.LoadSolution(paths.First());
        else
            await _sessionContext.LoadProjects(paths);
    }
}