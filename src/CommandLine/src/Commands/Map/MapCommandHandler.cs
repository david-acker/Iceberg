using Iceberg.CommandLine.Commands.Map.Options;
using Iceberg.Export;
using Iceberg.Map.DependencyMapper;
using Iceberg.Map.DependencyMapper.Context;
using Iceberg.Map.DependencyMapper.Filters.Projects;
using Iceberg.Map.Metadata;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Iceberg.CommandLine.Commands.Map;

// TODO: Consider defining this closer to the MapCommand instead, since the parameters are
// inherently coupled to how the options are setup in the command.
internal interface IMapCommandHandler
{
    Task Handle(
        string className,
        int? distance,
        MappingFlow mappingFlow,
        string methodName,
        string projectName);
}

internal sealed partial class MapCommandHandler : IMapCommandHandler
{
    private readonly IIcebergSessionContext _sessionContext;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MapCommandHandler> _logger;

    public MapCommandHandler(
        IIcebergSessionContext sessionContext,
        IServiceProvider serviceProvider,
        ILogger<MapCommandHandler> logger)
    {
        _sessionContext = sessionContext;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task Handle(
        string className,
        int? distance,
        MappingFlow mappingFlow,
        string methodName,
        string projectName)
    {
        if (!_sessionContext.IsSolutionLoaded
            || _sessionContext.Solution is null)
        {
            Log.NoSolutionLoaded(_logger);
            return;
        }

        var methodSolutionContext = _serviceProvider.GetService<IMethodSolutionContext>()!;

        var cancellationTokenSource = new CancellationTokenSource();

        IProjectFilter projectFilter = string.IsNullOrWhiteSpace(projectName)
            ? new DefaultProjectFilter()
            : new ProjectNameFilter(
                new[] { projectName },
                ProjectNameFilter.ProjectNameFilterType.Include);

        var matchingMethodEntryPoints =
            await methodSolutionContext.FindMethodEntryPoints(
                className,
                methodName,
                projectFilter,
                cancellationTokenSource.Token);

        if (!matchingMethodEntryPoints.Any())
        {
            Log.NoMatchingEntryPointsFound(_logger);
            return;
        }

        var methodDependencyMapper = _serviceProvider.GetService<IMethodDependencyMapper>()!;

        var methodDependencyMap =
            await GetMethodDependencyMap(
                mappingFlow,
                methodDependencyMapper,
                methodSolutionContext,
                matchingMethodEntryPoints,
                distance,
                cancellationTokenSource.Token);

        // TODO: Remove now that trimming is handled during mapping.
        if (distance.HasValue)
        {
            var entryPointMethodMetadata = methodDependencyMap.Keys
                .Where(x =>
                    methodName is null || x.MethodName.StartsWith(methodName)
                    && x.ClassName.StartsWith(className));

            methodDependencyMap = GetTrimmedDependencyMap(methodDependencyMap, entryPointMethodMetadata, distance.Value);
        }

        // TODO: Export this to a more accessible place while testing.
        // Currently written to Iceberg\src\CommandLine\src\bin\Debug\net6.0
        var outputFileName = "dependency_map.gen.dgml";

        var dgmlBuilderService = _serviceProvider.GetService<IDGMLBuilderService>()!;

        var dependencyMapName =
            string.IsNullOrWhiteSpace(methodName)
                ? className
                : $"{className}.{methodName}";

        // TODO: Don't overwrite generated maps.

        var exportedDGML = dgmlBuilderService.ExportDependencyMap(dependencyMapName, methodDependencyMap);

        exportedDGML.Save(outputFileName);
        _sessionContext.LastGeneratedDependencyMap = outputFileName;

        Log.ExportedDependencyMap(_logger, outputFileName);
    }

    private static async Task<MethodDependencyMap> GetMethodDependencyMap(
       MappingFlow mappingFlow,
       IMethodDependencyMapper methodDependencyMapper,
       IMethodSolutionContext solutionContext,
       IEnumerable<IEntryPoint<MethodDeclarationSyntax>> entryPoints,
       int? depth,
       CancellationToken cancellationToken)
    {
        // TODO: Handle zero or multiple matching entry points based on additional input from the user.
        // TODO: Add a separate subcommand or options to specify that an entire class should be mapped.

        if (mappingFlow == MappingFlow.Downstream)
        {
            // TODO: Provide via CLI option or solution-level configuration.
            var testProjectExclusionFilter = new ProjectNameContainsFilter(
                "Test",
                ProjectNameContainsFilter.ProjectNameContainsFilterType.Exclude);

            return await methodDependencyMapper.MapDownstream(
                solutionContext,
                entryPoints,
                testProjectExclusionFilter,
                cancellationToken: cancellationToken);
        }
        else if (mappingFlow == MappingFlow.Upstream)
        {
            return await methodDependencyMapper.MapUpstream(
                solutionContext,
                entryPoints,
                depth,
                cancellationToken);
        }

        Debug.Assert(false, "Invalid Mapping Flow Option.");
        return null!;
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
            Traverse(generatedMethodDependencyMap, trimmedMethodDependencyMap, dependencyMethodMetadata, remainingDegreeOfSeparation--);
        }
    }

    private static partial class Log
    {
        [LoggerMessage(1, LogLevel.Information, "No solution loaded.", EventName = "NoSolutionLoaded")]
        public static partial void NoSolutionLoaded(ILogger logger);

        [LoggerMessage(2, LogLevel.Information, "No matching entry points found.", EventName = "NoMatchingEntryPointsFound")]
        public static partial void NoMatchingEntryPointsFound(ILogger logger);

        [LoggerMessage(3, LogLevel.Information, "Exported generated dependency map {FileName}", EventName = "ExportedDependencyMap")]
        public static partial void ExportedDependencyMap(ILogger logger, string fileName);
    }
}