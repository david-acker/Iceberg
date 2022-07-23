using Microsoft.CodeAnalysis;

namespace Iceberg.Map.DependencyMapper.Wrappers;

public interface ISymbolEqualityComparerWrapper
{
    /// <summary>
    /// Determine if the two <see cref="ISymbol"/> instances are equal.
    /// </summary>
    /// <param name="x">The first <see cref="ISymbol"/>.</param>
    /// <param name="y">The second <see cref="ISymbol"/>.</param>
    /// <returns>Whether the provided symbols are equal.</returns>
    bool Equals(ISymbol? x, ISymbol? y);
}

/// <summary>
/// Wrapper class for the static <see cref="SymbolEqualityComparer"/>
/// </summary>
public class SymbolEqualityComparerWrapper : ISymbolEqualityComparerWrapper
{
    /// <inheritdoc />
    public bool Equals(ISymbol? x, ISymbol? y)
    {
        return SymbolEqualityComparer.Default.Equals(x, y);
    }
}
