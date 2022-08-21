namespace Iceberg.CommandLine.Commands.Map.Options;

internal interface IDistanceOptionParser : IOptionParser<int?>
{
}

internal class DistanceOptionParser : BaseOptionParser<int?>, IDistanceOptionParser
{
    public override string[] Aliases => new[] { "--distance", "-d" };
    public override string Description => "The maximum distance to map from the entry point.";

    public override bool AllowMultipleArgumentsPerToken => false;
    public override bool IsRequired => false;

    public override ArgumentParseResult<int?> Parse(IEnumerable<string> distanceValues)
    {
        var distanceValue = distanceValues.Single();

        if (string.IsNullOrWhiteSpace(distanceValue)
            || !int.TryParse(distanceValue, out int distance))
        {
            return Failure("The maximum distance must be a valid integer greater than zero.");
        }

        if (distance < 1)
        {
            return Failure("The maximum distance must be greater than zero.");
        }

        return Success(distance);
    }
}
