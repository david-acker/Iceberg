using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Iceberg.Map.DependencyMapper.Selectors;

public sealed class OverriddenMethodSelector : ISimpleMethodSelector
{
    public IEnumerable<ISymbol> GetSymbols(IEntryPoint<MethodDeclarationSyntax> entryPoint, IEnumerable<ISymbol> candidateSymbols)
    {
        if (entryPoint.Symbol is not IMethodSymbol entryPointMethodSymbol
            || !entryPointMethodSymbol.IsOverride
            || entryPointMethodSymbol.OverriddenMethod is null)
        {
            return new List<ISymbol>();
        }

        return candidateSymbols
            .Where(symbol => symbol.IsAbstract
                || symbol.IsVirtual
                && SymbolEqualityComparer.Default.Equals(symbol, entryPointMethodSymbol.OverriddenMethod)
                && !SymbolEqualityComparer.Default.Equals(symbol, entryPointMethodSymbol));
    }
}
