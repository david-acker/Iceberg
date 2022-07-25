using Microsoft.Extensions.Logging;
using System.CommandLine;

namespace Iceberg.CommandLine.Commands;

internal sealed partial class LoadCommand : ICommand
{
    private const string _name = "load";
    private const string _description = "Load the specified projects or solution.";

    public Command Value { get; }

    public LoadCommand(IIcebergSessionContext context, ILogger<LoadCommand> logger)
    {
        Value = CreateCommand(context, logger);
    }

    public static Command CreateCommand(IIcebergSessionContext context, ILogger<LoadCommand> logger)
    {
        var command = new Command(_name, _description);

        var pathOption = GetPathOption();
        command.Add(pathOption);

        command.SetHandler(async (pathOptionValue) =>
        {
            if (pathOptionValue.Length == 1)
            {
                await context.LoadSolution(pathOptionValue.Single());
            }
            else
            {
                await context.LoadProjects(pathOptionValue);
            }
        },
        pathOption);

        return command;
    }

    // TODO: Create a custom 'PathOption' class to better encapsulate this.
    private static Option<string[]> GetPathOption()
    {
        var supportedFileExtensions = new[] { ".sln", ".csproj" };

        return new Option<string[]>(
            aliases: new[] { "--path", "-p" },
            description: "The path to the solution file (.sln) or project file(s) (.csproj).",
            parseArgument: result =>
            {
                var paths = result.Tokens.Select(x => x.Value);

                if (paths.Count() > 1)
                {
                    if (paths.Any(x => x.EndsWith(".sln")))
                    {
                        result.ErrorMessage = "Only a single solution (.sln) may be provided.";
                        return Array.Empty<string>();
                    }
                }

                // TODO: Handle possible file exceptions.
                var fileInfo = paths.Select(x => new FileInfo(x));

                if (fileInfo.Any(x => 
                    !supportedFileExtensions.Any(y => !string.Equals(y, x.Extension, StringComparison.OrdinalIgnoreCase))))
                {
                    result.ErrorMessage = $"Only the following file extensions are supported: {string.Join(", ", supportedFileExtensions)}";
                    return Array.Empty<string>();
                }

                var nonExistentFiles = fileInfo.Where(x => !x.Exists);
                if (nonExistentFiles.Any())
                {
                    result.ErrorMessage = $"The following files were not found: {string.Join(", ", nonExistentFiles.Select(x => x.Name))}";
                    return Array.Empty<string>();
                }

                return paths.ToArray();
            })
        {
            AllowMultipleArgumentsPerToken = true,
            IsRequired = true
        };
    }
}
