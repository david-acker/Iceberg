using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using Iceberg.Map.DependencyMapper.Context;
using Iceberg.Map.DependencyMapper.Wrappers;

namespace Iceberg.Map.DependencyMapper.Selectors;

public interface IMethodImplementationSelector : IMethodSelector
{
    Task<IEnumerable<ISymbol>> GetSymbols(
        IEntryPoint<MethodDeclarationSyntax> entryPoint,
        IEnumerable<ISymbol> candidateSymbols,
        ISolutionWrapper solutionWrapper,
        CancellationToken cancellationToken = default);
}
