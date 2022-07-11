using Iceberg.Map.Metadata;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;

namespace Iceberg.Map.DependencyMapper.Context;

internal partial class MethodDependencyMappingContext
{
    private readonly ILogger<MethodDependencyMappingContext> _logger;
    private readonly MethodSolutionContext _solutionContext;

    public MethodDependencyMap DependencyMap { get; init; } 
        = new MethodDependencyMap(new MethodMetadataComparer());

    public MethodDependencyMappingContext(
        ILoggerFactory loggerFactory,
        MethodSolutionContext solutionContext)
    {
        _logger = loggerFactory.CreateLogger<MethodDependencyMappingContext>();
        _solutionContext = solutionContext;
    }

    public async Task MapEntryPoint(
        EntryPoint<MethodDeclarationSyntax> methodEntryPoint,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(methodEntryPoint.DisplayName))
        {
            Log.InvalidEntryPointName(_logger);
            return;
        }

        if (DependencyMap.Any(x => string.Equals(x.Key.DisplayName, methodEntryPoint.DisplayName)))
        {
            Log.DuplicateMethodEntryPoint(_logger, methodEntryPoint.DisplayName);
            return;
        }

        var entryPointMetadata = new MethodMetadata(
            methodEntryPoint.DisplayName,
            methodEntryPoint.SemanticModel.SyntaxTree.FilePath);

        var dependencyEntryPoints = new HashSet<EntryPoint<MethodDeclarationSyntax>>();
        var dependencyEntryPointMetadata = new HashSet<MethodMetadata>();

        await foreach (var dependencyEntryPoint in _solutionContext.FindDependencyEntryPoints(methodEntryPoint, cancellationToken))
        {
            if (dependencyEntryPoints.Any(x => string.Equals(x.DisplayName, dependencyEntryPoint.DisplayName)))
            {
                Log.DuplicateDependencyMethodEntryPoint(_logger, dependencyEntryPoint.DisplayName);
                continue;
            }

            dependencyEntryPoints.Add(dependencyEntryPoint);
            dependencyEntryPointMetadata.Add(
                new MethodMetadata(
                    dependencyEntryPoint.DisplayName, 
                    dependencyEntryPoint.SemanticModel.SyntaxTree.FilePath));
        }

        DependencyMap[entryPointMetadata] = dependencyEntryPointMetadata;

        foreach (var dependencyEntryPoint in dependencyEntryPoints)
        {
            await MapEntryPoint(dependencyEntryPoint, cancellationToken);
        }
    }

    private static partial class Log
    {
        [LoggerMessage(100, LogLevel.Information, "Encountered an entry point with a null or empty name. Skipping.", EventName = "InvalidEntryPointName")]
        public static partial void InvalidEntryPointName(ILogger logger);

        [LoggerMessage(101, LogLevel.Information, "Encountered duplicate method entry point {EntryPointDisplayName}. Skipping.", EventName = "DuplicateMethodEntryPoint")]
        public static partial void DuplicateMethodEntryPoint(ILogger logger, string entryPointDisplayName);

        [LoggerMessage(102, LogLevel.Information, "Encountered duplicate dependency method entry point {EntryPointDisplayName}. Skipping.", EventName = "DuplicateDependencyMethodEntryPoint")]
        public static partial void DuplicateDependencyMethodEntryPoint(ILogger logger, string entryPointDisplayName);
    }
}
