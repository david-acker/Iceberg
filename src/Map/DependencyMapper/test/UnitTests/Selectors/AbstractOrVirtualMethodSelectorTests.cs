using Iceberg.Map.DependencyMapper.Selectors;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Moq;

namespace Iceberg.Map.DependencyMapper.UnitTests.Selectors;

[System.Diagnostics.CodeAnalysis.SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1024:Symbols should be compared for equality", Justification = "<Pending>")]
public class AbstractOrVirtualMethodSelectorTests
{
    [Fact(Skip = "Issue with SymbolEqualityComparer")]
    public async Task GetSymbols_WithImplementations_ReturnsAbstractOrVirtualMethodSymbols()
    {
        // Arrange
        var mockEntryPoint = new Mock<IEntryPoint<MethodDeclarationSyntax>>();

        var candidateSymbols = new List<Mock<ISymbol>>
        {
            GetMockSymbol(isAbstract: true, isVirtual: false),
            GetMockSymbol(isAbstract: false, isVirtual: true),
            GetMockSymbol(isAbstract: false, isVirtual: false)
        };

        var matchingCandidateSymbols = candidateSymbols.Take(2);

        var mockSolution = new Mock<ISolutionWrapper>();
        var cancellationToken = new CancellationToken();

        var implementations = new List<Mock<ISymbol>>
        {
            GetMockSymbol(),
            GetMockSymbol()
        };

        var mockSymbolFinderWrapper = new Mock<ISymbolFinderWrapper>();
        mockSymbolFinderWrapper
            .SetupSequence(x => x.FindImplementations(It.IsAny<ISymbol>(), It.IsAny<ISolutionWrapper>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(implementations.Take(1).Select(x => x.Object))
            .ReturnsAsync(implementations.Skip(1).Take(1).Select(x => x.Object));

        var selector = new AbstractOrVirtualMethodSelector(mockSymbolFinderWrapper.Object);

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

        Assert.Equal(implementations.Count, results.Count());
        Assert.Equal(implementations.Select(x => x.Object).ToHashSet(), results.ToHashSet(), SymbolEqualityComparer.Default);
    }

    [Fact(Skip = "Issue with SymbolEqualityComparer")]
    public async Task GetSymbols_WithImplementions_DoesNotIncludeTheEntryPointItself()
    {
        // Arrange
        var mockEntryPoint = new Mock<IEntryPoint<MethodDeclarationSyntax>>();

        var mockSymbol = GetMockSymbol(isAbstract: true, isVirtual: false);

        var symbol = mockSymbol.Object;

        mockEntryPoint.SetupGet(x => x.Symbol)
            .Returns(symbol);

        var mockSolution = new Mock<ISolutionWrapper>();
        var cancellationToken = new CancellationToken();

        var mockSymbolFinderWrapper = new Mock<ISymbolFinderWrapper>();
        mockSymbolFinderWrapper
            .Setup(x => x.FindImplementations(It.IsAny<ISymbol>(), It.IsAny<ISolutionWrapper>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { symbol });

        var selector = new AbstractOrVirtualMethodSelector(mockSymbolFinderWrapper.Object);

        // Act
        var results = await selector.GetSymbols(
            mockEntryPoint.Object,
            new[] { symbol },
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
                It.Is<ISymbol>(y => y == mockSymbol.Object),
                It.Is<ISolutionWrapper>(y => y == mockSolution.Object),
                It.Is<CancellationToken>(y => y == cancellationToken)), Times.Once);

        Assert.Empty(results);
    }

    [Fact]
    public async Task GetSymbols_WithNoImplementations_ReturnsEmpty()
    {
        // Arrange
        var mockEntryPoint = new Mock<IEntryPoint<MethodDeclarationSyntax>>();

        var candidateSymbols = new List<Mock<ISymbol>>
        {
            GetMockSymbol(isAbstract: true, isVirtual: false),
            GetMockSymbol(isAbstract: false, isVirtual: true),
            GetMockSymbol(isAbstract: false, isVirtual: false)
        };

        var matchingCandidateSymbols = candidateSymbols.Take(2);

        var mockSolution = new Mock<ISolutionWrapper>();
        var cancellationToken = new CancellationToken();

        var mockSymbolFinderWrapper = new Mock<ISymbolFinderWrapper>();
        mockSymbolFinderWrapper
            .Setup(x => x.FindImplementations(It.IsAny<ISymbol>(), It.IsAny<ISolutionWrapper>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ISymbol>());

        var selector = new AbstractOrVirtualMethodSelector(mockSymbolFinderWrapper.Object);

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

    private static Mock<ISymbol> GetMockSymbol(bool isAbstract = false, bool isVirtual = false)
    {
        var mockSymbol = new Mock<ISymbol>();

        mockSymbol.SetupGet(x => x.IsAbstract).Returns(isAbstract);
        mockSymbol.SetupGet(x => x.IsVirtual).Returns(isVirtual);

        return mockSymbol;
    }
}
