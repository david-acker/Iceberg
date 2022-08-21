using Iceberg.CommandLine.Commands.Map.Options;
using System.Diagnostics.CodeAnalysis;

namespace Iceberg.CommandLine.Tests.Commands.Map.Options;

[ExcludeFromCodeCoverage]
public class MethodOptionParserTests
{
    [Fact]
    public void Parse()
    {
        // Arrange
        var methodName = "method-name";
        var methodOptionParser = new MethodOptionParser();

        // Act
        var parseResult = methodOptionParser.Parse(new[] { methodName });

        // Assert
        Assert.False(parseResult.HasError);
        Assert.NotNull(parseResult.Value);
        Assert.Equal(methodName, parseResult.Value);
    }

    [Theory]
    [InlineData(" ")]
    [InlineData(null)]
    public void Parse_MethodNameIsNullOrWhiteSpace_Fails(string methodName)
    {
        // Arrange
        var methodOptionParser = new MethodOptionParser();

        // Act
        var parseResult = methodOptionParser.Parse(new[] { methodName });

        // Assert
        Assert.True(parseResult.HasError);
        Assert.Equal("The method name cannot be blank.", parseResult.ErrorMessage);
    }
}
