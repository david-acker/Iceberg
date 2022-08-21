using Iceberg.CommandLine.Commands.View.Services;
using Microsoft.Extensions.Logging;

namespace Iceberg.CommandLine.Commands.View;

internal interface IViewCommandHandler
{
    void Handle();
}

internal sealed partial class ViewCommandHandler : IViewCommandHandler
{
    private readonly IIcebergSessionContext _sessionContext;
    private readonly IProcessService _processService;
    private readonly ILogger<ViewCommandHandler> _logger;

    public ViewCommandHandler(
        IIcebergSessionContext sessionContext, 
        IProcessService processService,
        ILogger<ViewCommandHandler> logger)
    {
        _sessionContext = sessionContext;
        _processService = processService;
        _logger = logger;
    }

    public void Handle()
    {
        if (_sessionContext.LastGeneratedDependencyMap is null)
        {
            Log.DependencyMapNotYetGenerated(_logger);
            return;
        }

        Log.OpeningDependencyMap(_logger, _sessionContext.LastGeneratedDependencyMap);

        _processService.OpenFileWithDefaultProgram(_sessionContext.LastGeneratedDependencyMap);
    }

    private static partial class Log
    {
        [LoggerMessage(1, LogLevel.Information, "No dependency graphs have been generated yet.", EventName = "DependencyMapNotYetGenerated")]
        public static partial void DependencyMapNotYetGenerated(ILogger logger);

        [LoggerMessage(2, LogLevel.Information, "Opening dependency map: {DependencyMapName}", EventName = "OpeningDependencyMap")]
        public static partial void OpeningDependencyMap(ILogger logger, string dependencyMapName);
    }
}
