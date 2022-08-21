namespace Iceberg.CommandLine.Commands.Map.Options;

internal interface IClassOptionParser : IOptionParser<string>
{
}

internal class ClassOptionParser : BaseOptionParser<string>, IClassOptionParser
{
    public override string[] Aliases => new[] { "--class", "-c" };
    public override string Description => "The class name to use as the entry point.";

    public override bool AllowMultipleArgumentsPerToken => false;
    public override bool IsRequired => false;

    public override ArgumentParseResult<string> Parse(IEnumerable<string> classNames)
    {
        var className = classNames.Single();

        if (string.IsNullOrWhiteSpace(className))
        {
            return Failure("The class name cannot be blank.");
        }

        return Success(className);
    }
}
