using Iceberg.Map.DependencyMapper.Context;
using Iceberg.Map.Metadata;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;

namespace Iceberg.Map.DependencyMapper;

public partial class MethodDependencyMapper
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<MethodDependencyMapper> _logger;

    public MethodDependencyMapper(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
        _logger = _loggerFactory.CreateLogger<MethodDependencyMapper>();
    }

    public async Task<MethodDependencyMap> MapUpstream(
        MethodSolutionContext solutionContext, 
        EntryPoint<MethodDeclarationSyntax> methodEntryPoint,
        CancellationToken cancellationToken = default)
    {
        Log.UpstreamMethodDependencyMappingStart(_logger);

        var methodDependencyMappingContext = new MethodDependencyMappingContext(_loggerFactory, solutionContext);

        await methodDependencyMappingContext.MapEntryPoint(methodEntryPoint, cancellationToken);

        Log.UpstreamMethodDependencyMappingEnd(_logger);

        return methodDependencyMappingContext.DependencyMap;
    }

    public async Task<MethodDependencyMap> MapDownstream(
        MethodSolutionContext solutionContext,
        EntryPoint<MethodDeclarationSyntax> methodEntryPoint,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    private static partial class Log
    {
        [LoggerMessage(1, LogLevel.Information, "Upstream method dependency mapping starting.", EventName = "UpstreamMethodDependencyMappingStart")]
        public static partial void UpstreamMethodDependencyMappingStart(ILogger logger);

        [LoggerMessage(2, LogLevel.Information, "Upstream method dependency mapping completed.", EventName = "UpstreamMethodDependencyMappingEnd")]
        public static partial void UpstreamMethodDependencyMappingEnd(ILogger logger);
    }
}
