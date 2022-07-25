using Iceberg.CommandLine.Commands;
using System.CommandLine;

namespace Iceberg.CommandLine;

public interface IIcebergSessionHandler
{
    RootCommand RootCommand { get; }
    Task Handle(string commandLineInput);
}

internal class IcebergSessionHandler : IIcebergSessionHandler
{
    public RootCommand RootCommand { get; }

    public IcebergSessionHandler(IEnumerable<ICommand> commands)
    {
        RootCommand = new RootCommand("Iceberg Dependency Mapper");

        foreach (var command in commands)
        {
            RootCommand.AddCommand(command.Value);
        }
    }

    public async Task Handle(string commandLineInput)
    {
        await RootCommand.InvokeAsync(commandLineInput);
    }
}
