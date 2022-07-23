using Iceberg.Map.DependencyMapper.Wrappers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Iceberg.Map.DependencyMapper.Selectors;

public sealed class AbstractOrVirtualMethodSelector : IMethodImplementationSelector
{
    private readonly ISymbolEqualityComparerWrapper _symbolEqualityComparerWrapper;
    private readonly ISymbolFinderWrapper _symbolFinderWrapper;

    public AbstractOrVirtualMethodSelector(
        ISymbolEqualityComparerWrapper symbolEqualityComparerWrapper,
        ISymbolFinderWrapper symbolFinderWrapper)
    {
        _symbolEqualityComparerWrapper = symbolEqualityComparerWrapper;
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
            .Where(symbol => !_symbolEqualityComparerWrapper.Equals(symbol, entryPoint.Symbol))
            .ToList();
    }

    private async Task<IEnumerable<ISymbol>> FindImplementations(
        IEnumerable<ISymbol> symbols,
        ISolutionWrapper solutionWrapper,
        CancellationToken cancellationToken = default)
    {
        var implementationSymbolTasks = symbols
            .Select(symbol => _symbolFinderWrapper.FindImplementations(symbol, solutionWrapper, cancellationToken));

        //return (await Task.WhenAll(implementationSymbolTasks)).SelectMany(symbols => symbols);

        var result = (await Task.WhenAll(implementationSymbolTasks)).SelectMany(symbols => symbols);
        return result;
    }
}