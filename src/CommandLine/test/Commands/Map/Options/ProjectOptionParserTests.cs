using Iceberg.CommandLine.Commands.Map.Options;
using System.Diagnostics.CodeAnalysis;

namespace Iceberg.CommandLine.Tests.Commands.Map.Options;

[ExcludeFromCodeCoverage]
public class ProjectOptionParserTests
{
    [Fact]
    public void Parse()
    {
        // Arrange
        var projectName = "ProjectName";
        var projectOptionParser = new ProjectOptionParser();

        // Act
        var parseResult = projectOptionParser.Parse(new[] { projectName });

        // Assert
        Assert.False(parseResult.HasError);
        Assert.NotNull(parseResult.Value);
        Assert.Equal(projectName, parseResult.Value);
    }

    [Theory]
    [InlineData(" ")]
    [InlineData(null)]
    public void Parse_ClassNameIsNullOrWhiteSpace_Fails(string projectName)
    {
        // Arrange
        var projectOptionParser = new ProjectOptionParser();

        // Act
        var parseResult = projectOptionParser.Parse(new[] { projectName });

        // Assert
        Assert.True(parseResult.HasError);
        Assert.Equal("The project name cannot be blank.", parseResult.ErrorMessage);
    }
}
