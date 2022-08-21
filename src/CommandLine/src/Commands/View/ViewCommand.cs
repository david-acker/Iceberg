using Iceberg.CommandLine.Commands.View;
using System.CommandLine;

namespace Iceberg.CommandLine.Commands;

internal sealed class ViewCommand : ICommand
{
    public string Name => "view";
    public string Description => "View the last generated dependency graph.";

    public Command Value { get; }

    public ViewCommand(IViewCommandHandler viewCommandHandler)
    {
        var command = new Command(Name, Description);

        command.SetHandler(() => viewCommandHandler.Handle());

        Value = command;
    }
}
