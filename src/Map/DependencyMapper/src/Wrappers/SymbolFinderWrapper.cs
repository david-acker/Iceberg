using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;

namespace Iceberg.Map.DependencyMapper.Wrappers;

public interface ISymbolFinderWrapper
{
    /// <summary>
    /// Find implementations of the <paramref name="symbol"/> in the provided <paramref name="solutionWrapper"/>.
    /// </summary>
    /// <param name="symbol">The <see cref="ISymbol"/> to search for.</param>
    /// <param name="solutionWrapper">The <see cref="ISolutionWrapper"/> to search in.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>Any implementations of the provided symbol.</returns>
    Task<IEnumerable<ISymbol>> FindImplementations(ISymbol symbol, ISolutionWrapper solutionWrapper, CancellationToken cancellationToken = default);

    /// <summary>
    /// Find references of the <paramref name="symbol"/> in the provided <paramref name="solutionWrapper"/>.
    /// </summary>
    /// <param name="symbol">The <see cref="ISymbol"/> to search for.</param>
    /// <param name="solutionWrapper">The <see cref="ISolutionWrapper"/> to search in.</param>
    /// <param name="cancellationToken">An optional cancellation token.</param>
    /// <returns>Any references of the provided symbol.</returns>
    Task<IEnumerable<ReferencedSymbol>> FindReferences(ISymbol symbol, ISolutionWrapper solutionWrapper, CancellationToken cancellationToken = default);
}

/// <summary>
/// Wrapper class for the static <see cref="SymbolFinder"/> class.
/// </summary>
public class SymbolFinderWrapper : ISymbolFinderWrapper
{
    /// <inheritdoc />
    public Task<IEnumerable<ISymbol>> FindImplementations(ISymbol symbol, ISolutionWrapper solutionWrapper, CancellationToken cancellationToken = default)
    {
        return SymbolFinder.FindImplementationsAsync(symbol, solutionWrapper.Solution, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public Task<IEnumerable<ReferencedSymbol>> FindReferences(ISymbol symbol, ISolutionWrapper solutionWrapper, CancellationToken cancellationToken = default)
    {
        return SymbolFinder.FindReferencesAsync(symbol, solutionWrapper.Solution, cancellationToken: cancellationToken);
    }
}
