namespace Iceberg.CommandLine.Commands.Map.Options;

internal interface IFlowOptionParser : IOptionParser<MappingFlow>
{
}

internal class FlowOptionParser : BaseOptionParser<MappingFlow>, IFlowOptionParser
{
    public override string[] Aliases => new[] { "--flow", "-f" };
    public override string Description => "The flow of the dependency map.";

    public override bool AllowMultipleArgumentsPerToken => false;
    public override bool IsRequired => true;

    public override ArgumentParseResult<MappingFlow> Parse(IEnumerable<string> flowNameValues)
    {
        var flowNameValue = flowNameValues.Single();

        if (string.Equals(flowNameValue, "downstream", StringComparison.OrdinalIgnoreCase)
            || string.Equals(flowNameValue, "d", StringComparison.OrdinalIgnoreCase))
        {
            return Success(MappingFlow.Downstream);
        }

        if (string.Equals(flowNameValue, "upstream", StringComparison.OrdinalIgnoreCase)
            || string.Equals(flowNameValue, "u", StringComparison.OrdinalIgnoreCase))
        {
            return Success(MappingFlow.Upstream);
        }

        return Failure("Unrecognized mapping flow name. Please provide a valid flow name: downstream/d, upstream/u");
    }
}
