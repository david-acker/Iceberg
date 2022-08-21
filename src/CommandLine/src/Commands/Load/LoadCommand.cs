using System.CommandLine;
using Iceberg.CommandLine.Commands.Load;
using Iceberg.CommandLine.Commands.Load.Options;

namespace Iceberg.CommandLine.Commands;

internal sealed class LoadCommand : ICommand
{
    public string Name => "load";
    public string Description => "Load the specified projects or solution.";

    public Command Value { get; }

    public LoadCommand(
        ILoadCommandHandler loadCommandHandler,
        IPathOptionParser pathOptionParser)
    {
        var command = new Command(Name, Description);

        var pathOption = pathOptionParser.CreateOption();
        command.Add(pathOption);

        command.SetHandler(
            async (pathOptionValue) => await loadCommandHandler.Handle(pathOptionValue),
            pathOption);

        Value = command;
    }
}
