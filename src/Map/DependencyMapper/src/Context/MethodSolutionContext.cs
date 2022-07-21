using Iceberg.Map.DependencyMapper.Selectors;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace Iceberg.Map.DependencyMapper.Context;

public sealed partial class MethodSolutionContext : BaseMemberSolutionContext<MethodDeclarationSyntax>
{
    private readonly ILogger _logger;

    // TODO: Use dependency injection.
    private readonly IMethodSelector[] _methodSelectors =
        new IMethodSelector[]
        {
            new AbstractOrVirtualMethodSelector(new SymbolFinderWrapper()),
            new ConcreteMethodSelector(),
            new OverriddenMethodSelector()
        };

    public MethodSolutionContext(ILogger logger, ISolutionWrapper solutionWrapper) : base(logger, solutionWrapper)
    {
        _logger = logger;
    }

    public MethodSolutionContext(ILoggerFactory loggerFactory, ISolutionWrapper solutionWrapper) 
        : base(loggerFactory, solutionWrapper)
    {
        _logger = loggerFactory.CreateLogger<MethodSolutionContext>();
    }

    public override async IAsyncEnumerable<IEntryPoint<MethodDeclarationSyntax>> FindDependencyEntryPoints(
        IEntryPoint<MethodDeclarationSyntax> entryPoint,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (entryPoint.Symbol is null)
        {
            Log.NoDeclaredSymbol(_logger);
            yield break;
        }

        var candidateMethodSymbols = entryPoint.SyntaxNode.DescendantNodes()
            .OfType<IdentifierNameSyntax>()
            .Select(node => entryPoint.SemanticModel.GetSymbolInfo(node))
            .Where(symbolInfo => symbolInfo.Symbol is not null
                && symbolInfo.Symbol.Kind == SymbolKind.Method
                && IsAssemblyContainedInSolution(symbolInfo.Symbol.ContainingAssembly.Name))
            .Select(symbolInfo => symbolInfo.Symbol!);

        foreach (var selectedMethodSymbolsTask in GetSelectedMethodSymbols(entryPoint, candidateMethodSymbols, cancellationToken))
        {
            await foreach (var dependencyEntryPoint in ConstructEntryPoints(await selectedMethodSymbolsTask, cancellationToken))
            {
                yield return dependencyEntryPoint;
            }
        }

        yield break;
    }

    private IEnumerable<Task<IEnumerable<ISymbol>>> GetSelectedMethodSymbols(
        IEntryPoint<MethodDeclarationSyntax> entryPoint,
        IEnumerable<ISymbol> candidateMethodSymbols,
        CancellationToken cancellationToken)
    {
        static Task<IEnumerable<ISymbol>> InvokeSelector(
            IMethodSelector methodSelector,
            IEntryPoint<MethodDeclarationSyntax> entryPoint,
            IEnumerable<ISymbol> candidateMethodSymbols,
            ISolutionWrapper solutionWrapper,
            CancellationToken cancellationToken)
        {
            switch (methodSelector)
            {
                case IMethodImplementationSelector methodImplementationSelector:
                    return methodImplementationSelector.GetSymbols(entryPoint, candidateMethodSymbols, solutionWrapper, cancellationToken);
                case ISimpleMethodSelector simpleMethodSelector:
                    return Task.FromResult(simpleMethodSelector.GetSymbols(entryPoint, candidateMethodSymbols));
                default:
                    return Task.FromResult(new List<ISymbol>().AsEnumerable());
            }
        }

        return _methodSelectors.Select(selector => InvokeSelector(selector, entryPoint, candidateMethodSymbols, SolutionWrapper, cancellationToken));
    }

    public async Task<IEnumerable<IEntryPoint<MethodDeclarationSyntax>>> FindMethodEntryPoints(
        string className, 
        string? methodName = null,
        string? projectName = null,
        CancellationToken cancellationToken = default)
    {
        Func<MethodDeclarationSyntax, SemanticModel, bool> predicate =
            (MethodDeclarationSyntax syntax, SemanticModel model) =>
            {
                if (methodName != null)
                {
                    var methodSymbol = model.GetDeclaredSymbol(syntax, cancellationToken);
                    if (methodSymbol == null
                        || !string.Equals(methodSymbol.Name, methodName, StringComparison.OrdinalIgnoreCase)
                        || syntax.Parent == null)
                    {
                        return false;
                    }
                }

                var parent = syntax.Parent;
                if (parent is InterfaceDeclarationSyntax)
                {
                    return false;
                }

                var classSymbol = model.GetDeclaredSymbol(parent!, cancellationToken);

                return classSymbol != null
                    && string.Equals(classSymbol.Name, className, StringComparison.OrdinalIgnoreCase);
            };

        var entryPointMatches = await FindEntryPoints(predicate, projectName, cancellationToken);

        return entryPointMatches.DistinctBy(x => x.DisplayName);
    }

    private static partial class Log
    {
        [LoggerMessage(1, LogLevel.Information, "Finding implementations for {SymbolName}.", EventName = "FindingImplementations")]
        public static partial void FindingImplementations(ILogger logger, string symbolName);

        [LoggerMessage(2, LogLevel.Information, "Finding overrides for {SymbolName}.", EventName = "FindingOverrides")]
        public static partial void FindingOverrides(ILogger logger, string symbolName);

        [LoggerMessage(3, LogLevel.Information, "No declared symbol for entry point. Skipping.", EventName = "NoDeclaredSymbol")]
        public static partial void NoDeclaredSymbol(ILogger logger);
    }
}
