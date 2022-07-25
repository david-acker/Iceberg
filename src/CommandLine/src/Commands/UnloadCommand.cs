using Microsoft.Extensions.Logging;
using System.CommandLine;

namespace Iceberg.CommandLine.Commands;

internal sealed partial class UnloadCommand : ICommand
{
    private const string _name = "unload";
    private const string _description = "Unload the current solution.";

    public Command Value { get; }

    public UnloadCommand(IIcebergSessionContext context, ILogger<UnloadCommand> logger)
    {
        Value = CreateCommand(context, logger);
    }

    public static Command CreateCommand(
        IIcebergSessionContext context, 
        ILogger<UnloadCommand> logger)
    {
        var command = new Command(_name, _description);

        command.SetHandler(() =>
        {
            if (context.IsSolutionLoaded)
            {
                context.UnloadSolution();
                Log.UnloadedSolution(logger);
            }
            else
            {
                Log.NoSolutionLoaded(logger);
            }
        });

        return command;
    }

    private static partial class Log
    {
        [LoggerMessage(1, LogLevel.Information, "Successfully unloaded the current solution.", EventName = "UnloadedSolution")]
        public static partial void UnloadedSolution(ILogger logger);

        [LoggerMessage(2, LogLevel.Information, "No solution is currently loaded.", EventName = "NoSolutionLoaded")]
        public static partial void NoSolutionLoaded(ILogger logger);
    }
}
