using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;

namespace Iceberg.Map.DependencyMapper.Selectors;

public interface ISimpleMethodSelector : IMethodSelector
{
    IEnumerable<ISymbol> GetSymbols(
        IEntryPoint<MethodDeclarationSyntax> entryPoint,
        IEnumerable<ISymbol> candidateSymbols);
}
