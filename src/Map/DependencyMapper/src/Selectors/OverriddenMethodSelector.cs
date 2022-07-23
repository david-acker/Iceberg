using Iceberg.Map.DependencyMapper.Wrappers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Iceberg.Map.DependencyMapper.Selectors;

public sealed class OverriddenMethodSelector : ISimpleMethodSelector
{
    private readonly ISymbolEqualityComparerWrapper _symbolEqualityComparerWrapper;
    public OverriddenMethodSelector(ISymbolEqualityComparerWrapper symbolEqualityComparerWrapper)
    {
        _symbolEqualityComparerWrapper = symbolEqualityComparerWrapper;
    }

    public IEnumerable<ISymbol> GetSymbols(IEntryPoint<MethodDeclarationSyntax> entryPoint, IEnumerable<ISymbol> candidateSymbols)
    {
        if (entryPoint.Symbol is not IMethodSymbol entryPointMethodSymbol
            || !entryPointMethodSymbol.IsOverride
            || entryPointMethodSymbol.OverriddenMethod is null)
        {
            return new List<ISymbol>();
        }

        return candidateSymbols.Where(symbol => IsOverriddenMethod(symbol, entryPointMethodSymbol)).ToList();
    }

    public bool IsOverriddenMethod(ISymbol candidateSymbol, IMethodSymbol entryPointMethodSymbol)
    {
        if (!candidateSymbol.IsAbstract && !candidateSymbol.IsVirtual)
            return false;

        if (_symbolEqualityComparerWrapper.Equals(candidateSymbol, entryPointMethodSymbol))
            return false;

        return _symbolEqualityComparerWrapper.Equals(candidateSymbol, entryPointMethodSymbol.OverriddenMethod);
    }
}
