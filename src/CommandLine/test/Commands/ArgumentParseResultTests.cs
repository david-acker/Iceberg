using Iceberg.CommandLine.Commands;
using System.Diagnostics.CodeAnalysis;

namespace Iceberg.CommandLine.Tests.Commands;

[ExcludeFromCodeCoverage]
public class ArgumentParseResultTests
{
    [Theory]
    [InlineData("Error Message", true)]
    [InlineData("  ", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void HasError_ReturnsExpectedValue(string errorMessage, bool expected)
    {
        // Arrange
        var parseResult = new ArgumentParseResult<int>(errorMessage);

        // Act
        var actual = parseResult.HasError;

        // Assert
        Assert.Equal(expected, actual);
    }
}
