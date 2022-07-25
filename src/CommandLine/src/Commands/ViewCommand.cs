using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.Diagnostics;

namespace Iceberg.CommandLine.Commands;

internal sealed partial class ViewCommand : ICommand
{
    private const string _name = "view";
    private const string _description = "View the last generated dependency graph.";

    public Command Value { get; }

    public ViewCommand(IIcebergSessionContext context, ILogger<ViewCommand> logger)
    {
        Value = CreateCommand(context, logger);
    }

    public static Command CreateCommand(IIcebergSessionContext context, ILogger<ViewCommand> logger)
    {
        var command = new Command(_name, _description);

        command.SetHandler(() =>
        {
            if (context.LastGeneratedDependencyMap is null)
            {
                Log.DependencyMapNotYetGenerated(logger);
                return;
            }

            Log.OpeningDependencyMap(logger, context.LastGeneratedDependencyMap);

            using var fileOpener = new Process();
            fileOpener.StartInfo.FileName = "explorer";
            fileOpener.StartInfo.Arguments = "\"" + context.LastGeneratedDependencyMap + "\"";
            fileOpener.Start();
        });

        return command;
    }

    private static partial class Log
    {
        [LoggerMessage(1, LogLevel.Information, "No dependency graphs have been generated yet.", EventName = "DependencyMapNotYetGenerated")]
        public static partial void DependencyMapNotYetGenerated(ILogger logger);

        [LoggerMessage(2, LogLevel.Information, "Opening dependency map: {DependencyMapName}", EventName = "OpeningDependencyMap")]
        public static partial void OpeningDependencyMap(ILogger logger, string dependencyMapName);
    }
}
