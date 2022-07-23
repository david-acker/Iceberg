using Iceberg.Map.DependencyMapper.Selectors;
using Iceberg.Map.DependencyMapper.Wrappers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace Iceberg.Map.DependencyMapper.Context;

// TODO: Consider splitting up into separate solution contexts for upstream and downstream mapping (e.g. UpstreamMethodSolutionContext)?
public sealed partial class MethodSolutionContext : BaseMemberSolutionContext<MethodDeclarationSyntax>
{
    private readonly ILogger _logger;

    // TODO: Use dependency injection.
    private readonly IMethodSelector[] _methodSelectors =
        new IMethodSelector[]
        {
            new AbstractOrVirtualMethodSelector(
                new SymbolEqualityComparerWrapper(),
                new SymbolFinderWrapper()),
            new ConcreteMethodSelector(),
            new OverriddenMethodSelector(
                new SymbolEqualityComparerWrapper())
        };

    private readonly ISymbolFinderWrapper _symbolFinder = new SymbolFinderWrapper();

    public MethodSolutionContext(ILogger logger, ISolutionWrapper solutionWrapper) : base(logger, solutionWrapper)
    {
        _logger = logger;
    }

    public MethodSolutionContext(ILoggerFactory loggerFactory, ISolutionWrapper solutionWrapper)
        : base(loggerFactory, solutionWrapper)
    {
        _logger = loggerFactory.CreateLogger<MethodSolutionContext>();
    }

    public override async IAsyncEnumerable<IEntryPoint<MethodDeclarationSyntax>> FindUpstreamDependencyEntryPoints(
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

    public override async IAsyncEnumerable<IEntryPoint<MethodDeclarationSyntax>> FindDownstreamDependencyEntryPoints(
        IEntryPoint<MethodDeclarationSyntax> entryPoint,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (entryPoint.Symbol is null)
        {
            Log.NoDeclaredSymbol(_logger);
            yield break;
        }

        var referencedSymbols = await _symbolFinder.FindReferences(entryPoint.Symbol, SolutionWrapper, cancellationToken);

        await foreach (var consumingMethodSymbol in GetConsumingMethodSymbols(referencedSymbols, SolutionWrapper, cancellationToken))
        {
            if (consumingMethodSymbol is null)
                continue;

            await foreach (var dependencyEntryPoint in ConstructEntryPoints(new[] { consumingMethodSymbol }, cancellationToken))
            {
                yield return dependencyEntryPoint;
            }
        }
    }

    private async IAsyncEnumerable<ISymbol?> GetConsumingMethodSymbols(
        IEnumerable<ReferencedSymbol> referencedSymbols,
        ISolutionWrapper solutionWrapper,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var referencedSymbol in referencedSymbols)
        {
            // TODO: Use an injected "ProjectFilter" to handle which projects are excluded.
            var sourceLocations = referencedSymbol.Locations.Where(location => !location.Document.Project.Name.Contains("Test"));

            foreach (var sourceLocation in sourceLocations)
            {
                yield return await GetConsumingMethodSymbol(sourceLocation, solutionWrapper, cancellationToken);
            }
        }
    }

    private async Task<ISymbol?> GetConsumingMethodSymbol(
        ReferenceLocation referenceLocation,
        ISolutionWrapper solutionWrapper,
        CancellationToken cancellationToken = default)
    {
        var location = referenceLocation.Location;
        if (location is null)
            return null;

        var rootTask = location.SourceTree?.GetRootAsync(cancellationToken);
        if (rootTask is null)
            return null;

        var referenceNode = (await rootTask).FindNode(location.SourceSpan);
        if (referenceNode is null)
            return null;

        var containingMethodDeclarationNode =
            referenceNode.FirstAncestorOrSelf((SyntaxNode syntaxNode) => syntaxNode is MethodDeclarationSyntax) as MethodDeclarationSyntax;
        if (containingMethodDeclarationNode is null
            || containingMethodDeclarationNode == referenceNode)
            return null;

        var containingDocument = solutionWrapper.Solution.GetDocument(containingMethodDeclarationNode.SyntaxTree);
        if (containingDocument is null)
            return null;

        var semanticModel = await containingDocument.GetSemanticModelAsync(cancellationToken);
        if (semanticModel is null)
            return null;

        return semanticModel.GetDeclaredSymbol(containingMethodDeclarationNode, cancellationToken);
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
