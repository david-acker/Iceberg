using Iceberg.Map.Metadata;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;

namespace Iceberg.Map.DependencyMapper.Context;

public interface IMethodDependencyMappingContext
{
    MethodDependencyMap DependencyMap { get; init; }

    Task MapUpstream(
        IEntryPoint<MethodDeclarationSyntax> methodEntryPoint,
        CancellationToken cancellationToken = default);

    Task MapDownstream(
        IEntryPoint<MethodDeclarationSyntax> methodEntryPoint,
        CancellationToken cancellationToken = default);
}

internal partial class MethodDependencyMappingContext : IMethodDependencyMappingContext
{
    private readonly ILogger<MethodDependencyMappingContext> _logger;
    private readonly IMethodSolutionContext _solutionContext;

    public MethodDependencyMap DependencyMap { get; init; } 
        = new MethodDependencyMap(new MethodMetadataComparer());

    public MethodDependencyMappingContext(
        ILoggerFactory loggerFactory,
        IMethodSolutionContext solutionContext)
    {
        _logger = loggerFactory.CreateLogger<MethodDependencyMappingContext>();
        _solutionContext = solutionContext;
    }

    public async Task MapUpstream(
        IEntryPoint<MethodDeclarationSyntax> methodEntryPoint,
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

        var dependencyEntryPoints = new HashSet<IEntryPoint<MethodDeclarationSyntax>>();
        var dependencyEntryPointMetadata = new HashSet<MethodMetadata>();

        await foreach (var dependencyEntryPoint in _solutionContext.FindUpstreamDependencyEntryPoints(methodEntryPoint, cancellationToken))
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
            await MapUpstream(dependencyEntryPoint, cancellationToken);
        }
    }

    public async Task MapDownstream(
        IEntryPoint<MethodDeclarationSyntax> methodEntryPoint,
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

        var dependencyEntryPoints = new HashSet<IEntryPoint<MethodDeclarationSyntax>>();
        var dependencyEntryPointMetadata = new HashSet<MethodMetadata>();

        await foreach (var dependencyEntryPoint in _solutionContext.FindDownstreamDependencyEntryPoints(methodEntryPoint, cancellationToken))
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
            await MapDownstream(dependencyEntryPoint, cancellationToken);
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
