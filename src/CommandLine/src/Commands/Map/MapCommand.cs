using Iceberg.CommandLine.Commands.Map;
using Iceberg.CommandLine.Commands.Map.Options;
using System.CommandLine;

namespace Iceberg.CommandLine.Commands;

internal class MapCommand : ICommand
{
    public string Name => "map";
    public string Description => "Map the dependencies for the specified entry point.";

    public Command Value { get; }

    public MapCommand(
        IMapCommandHandler mapCommandHandler,
        IClassOptionParser classOptionParser,
        IDistanceOptionParser distanceOptionParser,
        IFlowOptionParser flowOptionParser,
        IMethodOptionParser methodOptionParser,
        IProjectOptionParser projectOptionParser)
    {
        var command = new Command(Name, Description);

        var classOption = classOptionParser.CreateOption();
        command.Add(classOption);

        var distanceOption = distanceOptionParser.CreateOption();
        command.Add(distanceOption);

        var flowOption = flowOptionParser.CreateOption();
        command.Add(flowOption);

        var methodOption = methodOptionParser.CreateOption();
        command.Add(methodOption);

        var projectOption = projectOptionParser.CreateOption();
        command.Add(projectOption);

        command.SetHandler(
            async (classOptionValue, distanceOptionValue, flowOptionValue, methodOptionValue, projectOptionValue) =>
                await mapCommandHandler.Handle(classOptionValue, distanceOptionValue, flowOptionValue, methodOptionValue, projectOptionValue),
            classOption, distanceOption, flowOption, methodOption, projectOption);

        Value = command;
    }
}