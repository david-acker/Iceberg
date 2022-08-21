using Microsoft.Extensions.Logging;

namespace Iceberg.CommandLine.Commands.Unload;

internal interface IUnloadCommandHandler
{
    void Handle();
}

internal sealed partial class UnloadCommandHandler : IUnloadCommandHandler
{
    private readonly IIcebergSessionContext _sessionContext;
    private readonly ILogger<UnloadCommandHandler> _logger;

    public UnloadCommandHandler(
        IIcebergSessionContext sessionContext,
        ILogger<UnloadCommandHandler> logger)
    {
        _sessionContext = sessionContext;
        _logger = logger;
    }

    public void Handle()
    {
        if (_sessionContext.IsSolutionLoaded)
        {
            _sessionContext.UnloadSolution();
            Log.UnloadedSolution(_logger);
        }
        else
        {
            Log.NoSolutionLoaded(_logger);
        }
    }

    private static partial class Log
    {
        [LoggerMessage(1, LogLevel.Information, "Successfully unloaded the current solution.", EventName = "UnloadedSolution")]
        public static partial void UnloadedSolution(ILogger logger);

        [LoggerMessage(2, LogLevel.Information, "No solution is currently loaded.", EventName = "NoSolutionLoaded")]
        public static partial void NoSolutionLoaded(ILogger logger);
    }
}
