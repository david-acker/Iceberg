using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Iceberg.Map.DependencyMapper.Selectors;

public sealed class ConcreteMethodSelector : ISimpleMethodSelector
{
    public IEnumerable<ISymbol> GetSymbols(IEntryPoint<MethodDeclarationSyntax> entryPoint, IEnumerable<ISymbol> candidateSymbols)
    {
        return candidateSymbols
            .Where(symbol => !symbol.IsAbstract);
    }
}
