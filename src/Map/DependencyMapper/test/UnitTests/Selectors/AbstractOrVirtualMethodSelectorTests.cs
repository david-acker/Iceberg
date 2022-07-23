using Iceberg.Map.DependencyMapper.Selectors;
using Iceberg.Map.DependencyMapper.Wrappers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Moq;
using System.Diagnostics.CodeAnalysis;

namespace Iceberg.Map.DependencyMapper.UnitTests.Selectors;

[ExcludeFromCodeCoverage]
[SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1024:Symbols should be compared for equality", Justification = "<Pending>")]
public class AbstractOrVirtualMethodSelectorTests
{
    [Fact]
    public async Task GetSymbols_WithImplementations_ReturnsAbstractOrVirtualMethodSymbols()
    {
        // Arrange
        var mockEntryPoint = new Mock<IEntryPoint<MethodDeclarationSyntax>>();

        var mockEntryPointSymbol = TestUtilities.GetMockSymbol<ISymbol>();
        mockEntryPoint.SetupGet(x => x.Symbol)
            .Returns(mockEntryPointSymbol.Object);

        var candidateSymbols = new List<Mock<ISymbol>>
        {
            TestUtilities.GetMockSymbol<ISymbol>(isAbstract: true, isVirtual: false),
            TestUtilities.GetMockSymbol<ISymbol>(isAbstract: false, isVirtual: true),
            TestUtilities.GetMockSymbol<ISymbol>(isAbstract: false, isVirtual: false)
        };

        var matchingCandidateSymbols = candidateSymbols.Take(2);

        var mockSolution = new Mock<ISolutionWrapper>();
        var cancellationToken = new CancellationToken();

        var implementations = new List<Mock<ISymbol>>
        {
            TestUtilities.GetMockSymbol<ISymbol>(),
            TestUtilities.GetMockSymbol<ISymbol>()
        };

        var mockSymbolEqualityComparerWrapper = TestUtilities.GetMockSymbolEqualityComparerWrapper();

        var mockSymbolFinderWrapper = new Mock<ISymbolFinderWrapper>();
        mockSymbolFinderWrapper
            .SetupSequence(x => x.FindImplementations(It.IsAny<ISymbol>(), It.IsAny<ISolutionWrapper>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(implementations.Take(1).Select(x => x.Object))
            .ReturnsAsync(implementations.Skip(1).Take(1).Select(x => x.Object));

        var selector = new AbstractOrVirtualMethodSelector(
            mockSymbolEqualityComparerWrapper.Object,
            mockSymbolFinderWrapper.Object);

        // Act
        var results = await selector.GetSymbols(
            mockEntryPoint.Object,
            candidateSymbols.Select(x => x.Object),
            mockSolution.Object,
            cancellationToken);

        // Assert

        mockSymbolFinderWrapper
            .Verify(x => x.FindImplementations(
                It.IsAny<ISymbol>(),
                It.IsAny<ISolutionWrapper>(),
                It.IsAny<CancellationToken>()), Times.Exactly(matchingCandidateSymbols.Count()));

        foreach (var matchingSymbol in matchingCandidateSymbols)
        {
            mockSymbolFinderWrapper
                .Verify(x => x.FindImplementations(
                    It.Is<ISymbol>(y => y == matchingSymbol.Object),
                    It.Is<ISolutionWrapper>(y => y == mockSolution.Object),
                    It.Is<CancellationToken>(y => y == cancellationToken)), Times.Once);
        }

        mockSymbolEqualityComparerWrapper
            .Verify(x => x.Equals(It.IsAny<ISymbol?>(), It.IsAny<ISymbol?>()), Times.Exactly(implementations.Count));

        foreach (var implementation in implementations)
        {
            mockSymbolEqualityComparerWrapper
                .Verify(x => x.Equals(
                    It.Is<ISymbol?>(y => y == implementation.Object), 
                    It.Is<ISymbol?>(y => y == mockEntryPointSymbol.Object)), Times.Once);
        }

        Assert.Equal(implementations.Count, results.Count());
        
        foreach (var implementation in implementations.Select(x => x.Object))
        {
            Assert.Contains(results, x => x == implementation);
        }
    }

    [Fact]
    public async Task GetSymbols_WithImplementions_DoesNotIncludeTheEntryPointItself()
    {
        // Arrange
        var mockEntryPoint = new Mock<IEntryPoint<MethodDeclarationSyntax>>();

        var mockEntryPointSymbol = TestUtilities.GetMockSymbol<ISymbol>(isAbstract: true, isVirtual: false);
        mockEntryPoint.SetupGet(x => x.Symbol)
            .Returns(mockEntryPointSymbol.Object);

        var mockSolution = new Mock<ISolutionWrapper>();
        var cancellationToken = new CancellationToken();

        var mockSymbolEqualityComparerWrapper = TestUtilities.GetMockSymbolEqualityComparerWrapper();

        var mockSymbolFinderWrapper = new Mock<ISymbolFinderWrapper>();
        mockSymbolFinderWrapper
            .Setup(x => x.FindImplementations(It.IsAny<ISymbol>(), It.IsAny<ISolutionWrapper>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { mockEntryPointSymbol.Object });

        var selector = new AbstractOrVirtualMethodSelector(
            mockSymbolEqualityComparerWrapper.Object,
            mockSymbolFinderWrapper.Object);

        // Act
        var results = await selector.GetSymbols(
            mockEntryPoint.Object,
            new[] { mockEntryPointSymbol.Object },
            mockSolution.Object,
            cancellationToken);

        // Assert
        mockSymbolFinderWrapper
            .Verify(x => x.FindImplementations(
                It.IsAny<ISymbol>(),
                It.IsAny<ISolutionWrapper>(),
                It.IsAny<CancellationToken>()), Times.Once);

        mockSymbolFinderWrapper
            .Verify(x => x.FindImplementations(
                It.Is<ISymbol>(y => y == mockEntryPointSymbol.Object),
                It.Is<ISolutionWrapper>(y => y == mockSolution.Object),
                It.Is<CancellationToken>(y => y == cancellationToken)), Times.Once);

        mockSymbolEqualityComparerWrapper
           .Verify(x => x.Equals(It.IsAny<ISymbol?>(), It.IsAny<ISymbol?>()), Times.Once);

        mockSymbolEqualityComparerWrapper
            .Verify(x => x.Equals(
                It.Is<ISymbol?>(y => y == mockEntryPointSymbol.Object), 
                It.Is<ISymbol?>(y => y == mockEntryPointSymbol.Object)), Times.Once);

        Assert.Empty(results);
    }

    [Fact]
    public async Task GetSymbols_WithNoImplementations_ReturnsEmpty()
    {
        // Arrange
        var mockEntryPoint = new Mock<IEntryPoint<MethodDeclarationSyntax>>();

        var candidateSymbols = new List<Mock<ISymbol>>
        {
            TestUtilities.GetMockSymbol<ISymbol>(isAbstract: true, isVirtual: false),
            TestUtilities.GetMockSymbol<ISymbol>(isAbstract: false, isVirtual: true),
            TestUtilities.GetMockSymbol<ISymbol>(isAbstract: false, isVirtual: false)
        };

        var matchingCandidateSymbols = candidateSymbols.Take(2);

        var mockSolution = new Mock<ISolutionWrapper>();
        var cancellationToken = new CancellationToken();

        var mockSymbolEqualityComparerWrapper = TestUtilities.GetMockSymbolEqualityComparerWrapper();

        var mockSymbolFinderWrapper = new Mock<ISymbolFinderWrapper>();
        mockSymbolFinderWrapper
            .Setup(x => x.FindImplementations(It.IsAny<ISymbol>(), It.IsAny<ISolutionWrapper>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ISymbol>());

        var selector = new AbstractOrVirtualMethodSelector(
            mockSymbolEqualityComparerWrapper.Object,
            mockSymbolFinderWrapper.Object);

        // Act
        var results = await selector.GetSymbols(
            mockEntryPoint.Object,
            candidateSymbols.Select(x => x.Object),
            mockSolution.Object,
            cancellationToken);

        // Assert

        mockSymbolFinderWrapper
            .Verify(x => x.FindImplementations(
                It.IsAny<ISymbol>(),
                It.IsAny<ISolutionWrapper>(),
                It.IsAny<CancellationToken>()), Times.Exactly(matchingCandidateSymbols.Count()));

        foreach (var matchingSymbol in matchingCandidateSymbols)
        {
            mockSymbolFinderWrapper
                .Verify(x => x.FindImplementations(
                    It.Is<ISymbol>(y => y == matchingSymbol.Object),
                    It.Is<ISolutionWrapper>(y => y == mockSolution.Object),
                    It.Is<CancellationToken>(y => y == cancellationToken)), Times.Once);
        }

        Assert.Empty(results);
    }
}
