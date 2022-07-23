using Iceberg.Map.DependencyMapper.Wrappers;
using Microsoft.CodeAnalysis;
using Moq;
using System.Diagnostics.CodeAnalysis;

namespace Iceberg.Map.DependencyMapper.UnitTests.Selectors;

[ExcludeFromCodeCoverage]
internal static class TestUtilities
{
    public static Mock<TSymbol> GetMockSymbol<TSymbol>(bool isAbstract = false, bool isVirtual = false) where TSymbol : class, ISymbol
    {
        var mockSymbol = new Mock<TSymbol>();

        mockSymbol.SetupGet(x => x.IsAbstract).Returns(isAbstract);
        mockSymbol.SetupGet(x => x.IsVirtual).Returns(isVirtual);

        return mockSymbol;
    }

    public static Mock<ISymbolEqualityComparerWrapper> GetMockSymbolEqualityComparerWrapper()
    {
        var mockSymbolEqualityComparerWrapper = new Mock<ISymbolEqualityComparerWrapper>();

        // The built-in SymbolEqualityComparer does not work with mocked instances of ISymbol,
        // even when comparing the same instance of a mocked ISymbol. Checking for reference
        // equality gets around this.
        mockSymbolEqualityComparerWrapper
            .Setup(x => x.Equals(It.IsAny<ISymbol?>(), It.IsAny<ISymbol?>()))
            .Returns((ISymbol? x, ISymbol? y) =>
            {
#pragma warning disable RS1024 // Symbols should be compared for equality
                return x == y;
#pragma warning restore RS1024 // Symbols should be compared for equality
            });

        return mockSymbolEqualityComparerWrapper;
    }
}
