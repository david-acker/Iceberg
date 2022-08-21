using Iceberg.CommandLine.Commands.Load.Services;

namespace Iceberg.CommandLine.Commands.Load.Options;

public interface IPathOptionParser : IOptionParser<string[]>
{
}

public class PathOptionParser : BaseOptionParser<string[]>, IPathOptionParser
{
    private const string _projectFileExtension = ".csproj";
    private const string _solutionFileExtension = ".sln";

    public override string[] Aliases => new[] { "--path", "-p" };
    public override string Description => "The path to the solution file (.sln) or project file(s) (.csproj).";

    public override bool AllowMultipleArgumentsPerToken => true;
    public override bool IsRequired => true;

    private readonly IFileService _fileService;

    public PathOptionParser(IFileService fileService)
    {
        _fileService = fileService;
    }

    public override ArgumentParseResult<string[]> Parse(IEnumerable<string> paths)
    {
        if (paths.Count() > 1
            && paths.Any(x => x.EndsWith(_solutionFileExtension, StringComparison.OrdinalIgnoreCase)))
        {
            return Failure("Only a single solution (.sln) may be provided.");
        }

        if (paths.Any(x => 
            !x.EndsWith(_projectFileExtension, StringComparison.OrdinalIgnoreCase)
            && !x.EndsWith(_solutionFileExtension, StringComparison.OrdinalIgnoreCase)))
        {
            return Failure($"Only the following file extensions are supported: {_projectFileExtension}, {_solutionFileExtension}");
        }

        var filesNotFound = paths.Where(x => !_fileService.Exists(x));
        if (filesNotFound.Any())
        {
            return Failure($"The following files were not found: {string.Join(", ", filesNotFound)}");
        }

        return Success(paths.ToArray());
    }
}
