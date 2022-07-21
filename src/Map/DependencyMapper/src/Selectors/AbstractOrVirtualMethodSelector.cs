using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Iceberg.Map.DependencyMapper.Selectors;

public sealed class AbstractOrVirtualMethodSelector : IMethodImplementationSelector
{
    private readonly ISymbolFinderWrapper _symbolFinderWrapper;

    public AbstractOrVirtualMethodSelector(ISymbolFinderWrapper symbolFinderWrapper)
    {
        _symbolFinderWrapper = symbolFinderWrapper;
    }

    public async Task<IEnumerable<ISymbol>> GetSymbols(
        IEntryPoint<MethodDeclarationSyntax> entryPoint,
        IEnumerable<ISymbol> candidateSymbols,
        ISolutionWrapper solutionWrapper,
        CancellationToken cancellationToken = default)
    {
        var abstractOrVirtualMethodSymbols = candidateSymbols
            .Where(symbol => symbol.IsAbstract || symbol.IsVirtual);

        return (await FindImplementations(abstractOrVirtualMethodSymbols, solutionWrapper, cancellationToken))
            .Where(symbol => !SymbolEqualityComparer.Default.Equals(symbol, entryPoint.Symbol));
    }

    private async Task<IEnumerable<ISymbol>> FindImplementations(
        IEnumerable<ISymbol> symbols,
        ISolutionWrapper solutionWrapper,
        CancellationToken cancellationToken = default)
    {
        var implementationSymbolTasks = symbols
            .Select(symbol => _symbolFinderWrapper.FindImplementations(symbol, solutionWrapper, cancellationToken));

        return (await Task.WhenAll(implementationSymbolTasks)).SelectMany(symbols => symbols);
    }
}