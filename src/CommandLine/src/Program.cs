using Iceberg.Export;
using Iceberg.Map.DependencyMapper;
using Iceberg.Map.DependencyMapper.Context;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Extensions.Logging;

namespace Iceberg.CommandLine;

public class Program
{
    public static async Task Main(string[] args)
    {
        // TODO: Use dotnet-command-line in the future. While testing, can set the values 
        // using going to Debug > 'Iceberg.CommandLine Deubg Properties' and adding the values to
        // the 'Command Line Arguments' input box (separated by spaces).
        var solutionPath = args[0];
        var className = args[1];
        var methodName = args[2];

        // TODO: Consider moving this into some sort of workspace-specific infrastructure/helper service.
        if (!MSBuildLocator.IsRegistered)
        {
            var instances = MSBuildLocator.QueryVisualStudioInstances().ToArray();
            MSBuildLocator.RegisterInstance(instances.OrderByDescending(x => x.Version).First());
        }

        var workspace = MSBuildWorkspace.Create();
        var solution = await workspace.OpenSolutionAsync(solutionPath);

        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        });

        // TODO: Fix logging.
        var logger = loggerFactory.CreateLogger<Program>();

        // TODO: Hook this up to Console.CancelKeyPress
        var cancellationTokenSource = new CancellationTokenSource();

        var methodSolutionContext = new MethodSolutionContext(logger, solution);

        var matchingMethodEntryPoints = 
            await methodSolutionContext.FindMethodEntryPoints(
                className, 
                methodName, 
                cancellationTokenSource.Token);

        // TODO: Handle zero or multiple matching entry points based on additional input from the user.
        var selectedMethodEntryPoint = matchingMethodEntryPoints.Single();

        var methodDependencyMapper = new MethodDependencyMapper(loggerFactory);

        var upstreamMethodDependencyMap =
            await methodDependencyMapper.MapUpstream(
                methodSolutionContext,
                selectedMethodEntryPoint,
                cancellationTokenSource.Token);

        // TODO: Export this to a more accessible place while testing.
        // Currently written to Iceberg\src\CommandLine\src\bin\Debug\net6.0
        var outputFileName = "dependency_map.gen.dgml";

        var dgmlBuilderService = new DGMLBuilderService();
        var exportedDGML = dgmlBuilderService.ExportDependencyMap($"{className}.{methodName}", upstreamMethodDependencyMap);

        exportedDGML.Save(outputFileName);
        logger.LogInformation($"Saved generated dependency map to {outputFileName}");
    }
}