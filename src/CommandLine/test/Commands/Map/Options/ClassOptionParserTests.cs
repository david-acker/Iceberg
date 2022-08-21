using Iceberg.CommandLine.Commands.Map.Options;
using System.Diagnostics.CodeAnalysis;

namespace Iceberg.CommandLine.Tests.Commands.Map.Options;

[ExcludeFromCodeCoverage]
public class ClassOptionParserTests
{
    [Fact]
    public void Parse()
    {
        // Arrange
        var className = "class-name";
        var classOptionParser = new ClassOptionParser();

        // Act
        var parseResult = classOptionParser.Parse(new[] { className });

        // Assert
        Assert.False(parseResult.HasError);
        Assert.NotNull(parseResult.Value);
        Assert.Equal(className, parseResult.Value);
    }

    [Theory]
    [InlineData(" ")]
    [InlineData(null)]
    public void Parse_ClassNameIsNullOrWhiteSpace_Fails(string className)
    {
        // Arrange
        var classOptionParser = new ClassOptionParser();

        // Act
        var parseResult = classOptionParser.Parse(new[] { className });

        // Assert
        Assert.True(parseResult.HasError);
        Assert.Equal("The class name cannot be blank.", parseResult.ErrorMessage);
    }
}
