using Iceberg.Map.DependencyMapper.Selectors;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Moq;

namespace Iceberg.Map.DependencyMapper.UnitTests.Selectors;

public class ConcreteMethodSelectorTests
{
    [Fact]
    public void GetSymbols_ReturnsOnlyConcreteSymbols()
    {
        // Arrange
        var mockEntryPoint = new Mock<IEntryPoint<MethodDeclarationSyntax>>();

        var candidateSymbols = new List<Mock<ISymbol>>
        {
            GetMockSymbol(isAbstract: true),
            GetMockSymbol(isAbstract: false)
        };

        var selector = new ConcreteMethodSelector();

        // Act
        var results = selector.GetSymbols(
            mockEntryPoint.Object, 
            candidateSymbols.Select(x => x.Object));

        // Assert
        Assert.NotEmpty(results);
        Assert.True(results.All(x => !x.IsAbstract));
    }

    // TODO: Move to shared test utilities class.
    private static Mock<ISymbol> GetMockSymbol(bool isAbstract = false, bool isVirtual = false)
    {
        var mockSymbol = new Mock<ISymbol>();

        mockSymbol.SetupGet(x => x.IsAbstract).Returns(isAbstract);
        mockSymbol.SetupGet(x => x.IsVirtual).Returns(isVirtual);

        return mockSymbol;
    }
}
