using Iceberg.Map.DependencyMapper.Selectors;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Moq;
using System.Diagnostics.CodeAnalysis;

namespace Iceberg.Map.DependencyMapper.UnitTests.Selectors;

[ExcludeFromCodeCoverage]
public class OverriddenMethodSelectorTests
{
    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void GetSymbols_OverriddenMethodSymbols(bool isAbstract, bool isVirtual)
    {
        // Arrange
        var mockEntryPoint = new Mock<IEntryPoint<MethodDeclarationSyntax>>();

        var mockCandidateSymbol = TestUtilities.GetMockSymbol<IMethodSymbol>(isAbstract, isVirtual);

        var mockEntryPointMethodSymbol = new Mock<IMethodSymbol>();
        mockEntryPointMethodSymbol.SetupGet(x => x.IsOverride)
            .Returns(true);
        mockEntryPointMethodSymbol.Setup(x => x.OverriddenMethod)
            .Returns(mockCandidateSymbol.Object);

        mockEntryPoint.SetupGet(x => x.Symbol)
            .Returns(mockEntryPointMethodSymbol.Object);

        var mockSymbolEqualityComparerWrapper = TestUtilities.GetMockSymbolEqualityComparerWrapper();

        var selector = new OverriddenMethodSelector(mockSymbolEqualityComparerWrapper.Object);

        // Act
        var results = selector.GetSymbols(
            mockEntryPoint.Object,
            new[] { mockCandidateSymbol.Object });

        // Assert

        if (isAbstract || isVirtual)
        {
            mockSymbolEqualityComparerWrapper
                .Verify(x => x.Equals(It.IsAny<ISymbol?>(), It.IsAny<ISymbol?>()), Times.AtLeastOnce);

            Assert.Single(results, mockCandidateSymbol.Object);
        }
        else
        {
            mockSymbolEqualityComparerWrapper
                .Verify(x => x.Equals(It.IsAny<ISymbol?>(), It.IsAny<ISymbol?>()), Times.Never);

            Assert.Empty(results);
        }
    }

    [Fact]
    public void GetSymbols_OnlyIncludesTheOverriddenMethod()
    {
        // Arrange
        var mockEntryPoint = new Mock<IEntryPoint<MethodDeclarationSyntax>>();

        var mockCandidateSymbol = TestUtilities.GetMockSymbol<IMethodSymbol>(isAbstract: true);

        var mockEntryPointMethodSymbol = new Mock<IMethodSymbol>();
        mockEntryPointMethodSymbol.SetupGet(x => x.IsOverride)
            .Returns(true);

        var mockOverriddenMethodSymbol = TestUtilities.GetMockSymbol<IMethodSymbol>();
        mockEntryPointMethodSymbol.Setup(x => x.OverriddenMethod)
            .Returns(mockOverriddenMethodSymbol.Object);

        mockEntryPoint.SetupGet(x => x.Symbol)
            .Returns(mockEntryPointMethodSymbol.Object);

        var mockSymbolEqualityComparerWrapper = TestUtilities.GetMockSymbolEqualityComparerWrapper();

        var selector = new OverriddenMethodSelector(mockSymbolEqualityComparerWrapper.Object);

        // Act
        var results = selector.GetSymbols(
            mockEntryPoint.Object,
            new[] { mockCandidateSymbol.Object });

        // Assert
        mockSymbolEqualityComparerWrapper
            .Verify(x => x.Equals(It.IsAny<ISymbol?>(), It.IsAny<ISymbol?>()), Times.AtLeastOnce);

        Assert.Empty(results);
    }

    [Fact]
    public void GetSymbols_ExcludesTheEntryPointItself()
    {
        // Arrange
        var mockEntryPoint = new Mock<IEntryPoint<MethodDeclarationSyntax>>();

        var mockCandidateSymbol = TestUtilities.GetMockSymbol<IMethodSymbol>(isAbstract: true);

        var mockEntryPointMethodSymbol = new Mock<IMethodSymbol>();
        mockEntryPointMethodSymbol.SetupGet(x => x.IsOverride)
            .Returns(true);
        mockEntryPointMethodSymbol.Setup(x => x.OverriddenMethod)
            .Returns(mockCandidateSymbol.Object);

        mockEntryPoint.SetupGet(x => x.Symbol)
            .Returns(mockCandidateSymbol.Object);

        var candidateSymbols = new List<Mock<ISymbol>>
        {
            TestUtilities.GetMockSymbol<ISymbol>(isAbstract: true, isVirtual: false),
            TestUtilities.GetMockSymbol<ISymbol>(isAbstract: true, isVirtual: false),
            TestUtilities.GetMockSymbol<ISymbol>(isAbstract: false, isVirtual: true),
            TestUtilities.GetMockSymbol<ISymbol>(isAbstract: false, isVirtual: false)
        };

        var mockSymbolEqualityComparerWrapper = TestUtilities.GetMockSymbolEqualityComparerWrapper();

        var selector = new OverriddenMethodSelector(mockSymbolEqualityComparerWrapper.Object);

        // Act
        var results = selector.GetSymbols(
            mockEntryPoint.Object,
            candidateSymbols.Select(x => x.Object));

        // Assert
        mockSymbolEqualityComparerWrapper
            .Verify(x => x.Equals(It.IsAny<ISymbol?>(), It.IsAny<ISymbol?>()), Times.Never);

        Assert.Empty(results);
    }

    [Fact]
    public void GetSymbols_EntryPointIsNotMethodSymbol_ReturnsEmpty()
    {
        // Arrange
        var mockEntryPoint = new Mock<IEntryPoint<MethodDeclarationSyntax>>();

        var mockEntryPointMethodSymbol = new Mock<ISymbol>();

        mockEntryPoint.SetupGet(x => x.Symbol)
            .Returns(mockEntryPointMethodSymbol.Object);

        var candidateSymbols = new List<Mock<ISymbol>>
        {
            TestUtilities.GetMockSymbol<ISymbol>(isAbstract: true),
            TestUtilities.GetMockSymbol<ISymbol>(isAbstract: false)
        };

        var mockSymbolEqualityComparerWrapper = TestUtilities.GetMockSymbolEqualityComparerWrapper();

        var selector = new OverriddenMethodSelector(mockSymbolEqualityComparerWrapper.Object);

        // Act
        var results = selector.GetSymbols(
            mockEntryPoint.Object,
            candidateSymbols.Select(x => x.Object));

        // Assert
        mockSymbolEqualityComparerWrapper
            .Verify(x => x.Equals(It.IsAny<ISymbol?>(), It.IsAny<ISymbol?>()), Times.Never);

        Assert.Empty(results);
    }

    [Fact]
    public void GetSymbols_EntryPointMethodCannotBeOverridden_ReturnsEmpty()
    {
        // Arrange
        var mockEntryPoint = new Mock<IEntryPoint<MethodDeclarationSyntax>>();

        var mockEntryPointMethodSymbol = new Mock<IMethodSymbol>();
        mockEntryPointMethodSymbol.SetupGet(x => x.IsOverride)
            .Returns(false);

        mockEntryPointMethodSymbol.SetupGet(x => x.OverriddenMethod)
            .Returns(new Mock<IMethodSymbol>().Object);

        mockEntryPoint.SetupGet(x => x.Symbol)
            .Returns(mockEntryPointMethodSymbol.Object);

        var candidateSymbols = new List<Mock<ISymbol>>
        {
            TestUtilities.GetMockSymbol<ISymbol>(isAbstract: true),
            TestUtilities.GetMockSymbol<ISymbol>(isAbstract: false)
        };

        var mockSymbolEqualityComparerWrapper = TestUtilities.GetMockSymbolEqualityComparerWrapper();

        var selector = new OverriddenMethodSelector(mockSymbolEqualityComparerWrapper.Object);

        // Act
        var results = selector.GetSymbols(
            mockEntryPoint.Object,
            candidateSymbols.Select(x => x.Object));

        // Assert
        mockSymbolEqualityComparerWrapper
            .Verify(x => x.Equals(It.IsAny<ISymbol?>(), It.IsAny<ISymbol?>()), Times.Never);

        Assert.Empty(results);
    }

    [Fact]
    public void GetSymbols_OverriddenMethodIsNull_ReturnsEmpty()
    {
        // Arrange
        var mockEntryPoint = new Mock<IEntryPoint<MethodDeclarationSyntax>>();

        var mockEntryPointMethodSymbol = new Mock<IMethodSymbol>();
        mockEntryPointMethodSymbol.SetupGet(x => x.IsOverride)
            .Returns(true);

        mockEntryPointMethodSymbol.SetupGet(x => x.OverriddenMethod)
            .Returns(null as IMethodSymbol);

        mockEntryPoint.SetupGet(x => x.Symbol)
            .Returns(mockEntryPointMethodSymbol.Object);

        var candidateSymbols = new List<Mock<ISymbol>>
        {
            TestUtilities.GetMockSymbol<ISymbol>(isAbstract: true),
            TestUtilities.GetMockSymbol<ISymbol>(isAbstract: false)
        };

        var mockSymbolEqualityComparerWrapper = TestUtilities.GetMockSymbolEqualityComparerWrapper();

        var selector = new OverriddenMethodSelector(mockSymbolEqualityComparerWrapper.Object);

        // Act
        var results = selector.GetSymbols(
            mockEntryPoint.Object,
            candidateSymbols.Select(x => x.Object));

        // Assert
        mockSymbolEqualityComparerWrapper
            .Verify(x => x.Equals(It.IsAny<ISymbol?>(), It.IsAny<ISymbol?>()), Times.Never);

        Assert.Empty(results);
    }
}
