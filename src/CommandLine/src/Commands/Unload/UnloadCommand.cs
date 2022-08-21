using Iceberg.CommandLine.Commands.Unload;
using System.CommandLine;

namespace Iceberg.CommandLine.Commands;

internal sealed class UnloadCommand : ICommand
{
    public string Name => "unload";
    public string Description => "Unload the current solution.";

    public Command Value { get; }

    public UnloadCommand(IUnloadCommandHandler unloadCommandHandler)
    {
        var command = new Command(Name, Description);

        command.SetHandler(() => unloadCommandHandler.Handle());

        Value = command;
    }
}
