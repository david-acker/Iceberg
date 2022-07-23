using Iceberg.Export;
using Iceberg.Map.DependencyMapper;
using Iceberg.Map.DependencyMapper.Context;
using Iceberg.Map.DependencyMapper.Wrappers;
using Iceberg.Map.Metadata;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Extensions.Logging;

namespace Iceberg.CommandLine;

public class Program
{
    public static async Task Main(string[] args)
    {
        // TODO: Use dotnet-command-line in the future. While testing, can set the values 
        // using going to Debug > 'Iceberg.CommandLine Debug Properties' and adding the values to
        // the 'Command Line Arguments' input box (separated by spaces).
        string solutionPath = args[0];
        string className = args[1];
        string? methodName = args[2];
        string? projectName = args[3];
        int? maximumDegreeOfSeparation = null;

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

        var methodSolutionContext = new MethodSolutionContext(logger, new SolutionWrapper(solution));

        var matchingMethodEntryPoints =
            await methodSolutionContext.FindMethodEntryPoints(
                className,
                methodName,
                projectName,
                cancellationTokenSource.Token);

        // TODO: Handle zero or multiple matching entry points based on additional input from the user.
        // TODO: Add a separate subcommand or options to specify that an entire class should be mapped.

        var methodDependencyMapper = new MethodDependencyMapper(loggerFactory);

        var methodDependencyMap = await methodDependencyMapper.MapUpstream(
            methodSolutionContext, 
            matchingMethodEntryPoints, 
            cancellationTokenSource.Token);

        if (maximumDegreeOfSeparation > 0)
        {
            var entryPointMethodMetadata = methodDependencyMap.Keys
                .Where(x =>
                    methodName is null || x.MethodName.StartsWith(methodName)
                    && x.ClassName.StartsWith(className));

            methodDependencyMap = GetTrimmedDependencyMap(methodDependencyMap, entryPointMethodMetadata, maximumDegreeOfSeparation.Value);
        }

        // TODO: Export this to a more accessible place while testing.
        // Currently written to Iceberg\src\CommandLine\src\bin\Debug\net6.0
        var outputFileName = "dependency_map.gen.dgml";

        var dgmlBuilderService = new DGMLBuilderService();
        var dependencyMapName =
            string.IsNullOrWhiteSpace(methodName) 
                ? className 
                : $"{className}.{methodName}";
        var exportedDGML = dgmlBuilderService.ExportDependencyMap(dependencyMapName, methodDependencyMap);

        exportedDGML.Save(outputFileName);
        logger.LogInformation($"Saved generated dependency map to {outputFileName}");
    }

    // TODO: Handle this logic in the MapUpstream/MapDownstream methods in BaseMemberSolutionContext
    // so that dependencies past the maximum distance from the entry point aren't unnecessarily mapped.
    private static MethodDependencyMap GetTrimmedDependencyMap(
        MethodDependencyMap generatedMethodDependencyMap,
        IEnumerable<MethodMetadata> entryPointMethodMetadata,
        int maximumDegreeOfSeparation)
    {
        var trimmedMethodDependencyMap = new MethodDependencyMap();

        foreach (var methodMetadata in entryPointMethodMetadata)
        {
            Traverse(generatedMethodDependencyMap, trimmedMethodDependencyMap, methodMetadata, maximumDegreeOfSeparation);
        }

        return trimmedMethodDependencyMap;
    }

    private static void Traverse(
        MethodDependencyMap generatedMethodDependencyMap,
        MethodDependencyMap trimmedMethodDependencyMap,
        MethodMetadata entryPointMethodMetadata,
        int remainingDegreeOfSeparation)
    {
        if (trimmedMethodDependencyMap.ContainsKey(entryPointMethodMetadata))
            return;

        if (remainingDegreeOfSeparation < 1)
        {
            trimmedMethodDependencyMap[entryPointMethodMetadata] = new HashSet<MethodMetadata>();
            return;
        }

        trimmedMethodDependencyMap[entryPointMethodMetadata] = generatedMethodDependencyMap[entryPointMethodMetadata];

        foreach (var dependencyMethodMetadata in generatedMethodDependencyMap[entryPointMethodMetadata])
        {
            Traverse(generatedMethodDependencyMap, trimmedMethodDependencyMap, dependencyMethodMetadata, remainingDegreeOfSeparation);
        }
    }
}