using Iceberg.CommandLine.Commands.Map.Options;
using System.Diagnostics.CodeAnalysis;

namespace Iceberg.CommandLine.Tests.Commands.Map.Options;

[ExcludeFromCodeCoverage]
public class DistanceOptionParserTests
{
    [Fact]
    public void Parse()
    {
        // Arrange
        var distanceValue = "10";
        var distanceOptionParser = new DistanceOptionParser();

        // Act
        var parseResult = distanceOptionParser.Parse(new[] { distanceValue });

        // Assert
        Assert.False(parseResult.HasError);
        Assert.Equal(10, parseResult.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(" ")]
    [InlineData("abc")]
    public void Parse_NotAValidInteger_Fails(string distanceValue)
    {
        // Arrange
        var distanceOptionParser = new DistanceOptionParser();

        // Act
        var parseResult = distanceOptionParser.Parse(new[] { distanceValue });

        // Assert
        Assert.True(parseResult.HasError);
        Assert.Equal("The maximum distance must be a valid integer greater than zero.", parseResult.ErrorMessage);
    }

    [Theory]
    [InlineData("-1")]
    [InlineData("0")]
    public void Parse_DistanceLessThanOne_Fails(string distanceValue)
    {
        // Arrange
        var distanceOptionParser = new DistanceOptionParser();

        // Act
        var parseResult = distanceOptionParser.Parse(new[] { distanceValue });

        // Assert
        Assert.True(parseResult.HasError);
        Assert.Equal("The maximum distance must be greater than zero.", parseResult.ErrorMessage);
    }
}
