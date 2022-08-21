namespace Iceberg.CommandLine.Commands.Map.Options;

internal interface IProjectOptionParser : IOptionParser<string>
{ 
}

internal class ProjectOptionParser : BaseOptionParser<string>, IProjectOptionParser
{
    public override string[] Aliases => new[] { "--project", "-p" };
    public override string Description => "The project where the entry point is located.";

    public override bool AllowMultipleArgumentsPerToken => false;
    public override bool IsRequired => false;

    public override ArgumentParseResult<string> Parse(IEnumerable<string> projectNames)
    {
        var projectName = projectNames.Single();

        if (string.IsNullOrWhiteSpace(projectName))
        {
            return Failure("The project name cannot be blank.");
        }

        return Success(projectName);
    }
}
