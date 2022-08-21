namespace Iceberg.CommandLine.Commands.Map.Options;

internal interface IMethodOptionParser : IOptionParser<string>
{
}

internal class MethodOptionParser : BaseOptionParser<string>, IMethodOptionParser
{
    public override string[] Aliases => new[] { "--method", "-m" };
    public override string Description => "The method name to use as the entry point.";

    public override bool AllowMultipleArgumentsPerToken => false;
    public override bool IsRequired => false;

    public override ArgumentParseResult<string> Parse(IEnumerable<string> methodNames)
    {
        var methodName = methodNames.Single();

        if (string.IsNullOrWhiteSpace(methodName))
        {
            return Failure("The method name cannot be blank.");
        }

        return Success(methodName);
    }
}
