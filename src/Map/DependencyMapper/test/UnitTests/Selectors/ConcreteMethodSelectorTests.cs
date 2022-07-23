using Iceberg.Map.DependencyMapper.Selectors;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Moq;
using System.Diagnostics.CodeAnalysis;

namespace Iceberg.Map.DependencyMapper.UnitTests.Selectors;

[ExcludeFromCodeCoverage]
public class ConcreteMethodSelectorTests
{
    [Fact]
    public void GetSymbols_ReturnsOnlyConcreteSymbols()
    {
        // Arrange
        var mockEntryPoint = new Mock<IEntryPoint<MethodDeclarationSyntax>>();

        var candidateSymbols = new List<Mock<ISymbol>>
        {
            TestUtilities.GetMockSymbol<ISymbol>(isAbstract: true),
            TestUtilities.GetMockSymbol<ISymbol>(isAbstract: false)
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
}
