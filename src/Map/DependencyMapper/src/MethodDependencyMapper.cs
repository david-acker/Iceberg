using Iceberg.Map.DependencyMapper.Context;
using Iceberg.Map.Metadata;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;

namespace Iceberg.Map.DependencyMapper;

public interface IMethodDependencyMapper
{
    Task<MethodDependencyMap> MapUpstream(
        IMethodSolutionContext solutionContext,
        IEntryPoint<MethodDeclarationSyntax> methodEntryPoint,
        CancellationToken cancellationToken = default);

    Task<MethodDependencyMap> MapUpstream(
        IMethodSolutionContext solutionContext,
        IEnumerable<IEntryPoint<MethodDeclarationSyntax>> methodEntryPoints,
        CancellationToken cancellationToken = default);

    Task<MethodDependencyMap> MapDownstream(
        IMethodSolutionContext solutionContext,
        IEnumerable<IEntryPoint<MethodDeclarationSyntax>> methodEntryPoints,
        CancellationToken cancellationToken = default);
}

public sealed partial class MethodDependencyMapper : IMethodDependencyMapper
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<MethodDependencyMapper> _logger;

    public MethodDependencyMapper(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
        _logger = _loggerFactory.CreateLogger<MethodDependencyMapper>();
    }

    public async Task<MethodDependencyMap> MapUpstream(
        IMethodSolutionContext solutionContext, 
        IEntryPoint<MethodDeclarationSyntax> methodEntryPoint,
        CancellationToken cancellationToken = default)
    {
        return await MapUpstream(solutionContext, new[] { methodEntryPoint }, cancellationToken);
    }

    public async Task<MethodDependencyMap> MapUpstream(
       IMethodSolutionContext solutionContext,
       IEnumerable<IEntryPoint<MethodDeclarationSyntax>> methodEntryPoints,
       CancellationToken cancellationToken = default)
    {
        Log.UpstreamMethodDependencyMappingStart(_logger);

        var methodDependencyMappingContext = new MethodDependencyMappingContext(_loggerFactory, solutionContext);

        foreach (var entryPoint in methodEntryPoints)
        {
            await methodDependencyMappingContext.MapUpstream(entryPoint, cancellationToken);
        }
        
        Log.UpstreamMethodDependencyMappingEnd(_logger);

        return methodDependencyMappingContext.DependencyMap;
    }

    public async Task<MethodDependencyMap> MapDownstream(
        IMethodSolutionContext solutionContext,
        IEnumerable<IEntryPoint<MethodDeclarationSyntax>> methodEntryPoints,
        CancellationToken cancellationToken = default)
    {
        Log.DownstreamMethodDependencyMappingStart(_logger);

        var methodDependencyMappingContext = new MethodDependencyMappingContext(_loggerFactory, solutionContext);

        foreach (var entryPoint in methodEntryPoints)
        {
            await methodDependencyMappingContext.MapDownstream(entryPoint, cancellationToken);
        }

        Log.DownstreamMethodDependencyMappingEnd(_logger);

        return methodDependencyMappingContext.DependencyMap;
    }

    private static partial class Log
    {
        [LoggerMessage(1, LogLevel.Information, "Upstream method dependency mapping starting.", EventName = "UpstreamMethodDependencyMappingStart")]
        public static partial void UpstreamMethodDependencyMappingStart(ILogger logger);

        [LoggerMessage(2, LogLevel.Information, "Upstream method dependency mapping completed.", EventName = "UpstreamMethodDependencyMappingEnd")]
        public static partial void UpstreamMethodDependencyMappingEnd(ILogger logger);

        [LoggerMessage(3, LogLevel.Information, "Downstream method dependency mapping starting.", EventName = "DownstreamMethodDependencyMappingStart")]
        public static partial void DownstreamMethodDependencyMappingStart(ILogger logger);

        [LoggerMessage(4, LogLevel.Information, "Downstream method dependency mapping completed.", EventName = "DownstreamMethodDependencyMappingEnd")]
        public static partial void DownstreamMethodDependencyMappingEnd(ILogger logger);
    }
}
