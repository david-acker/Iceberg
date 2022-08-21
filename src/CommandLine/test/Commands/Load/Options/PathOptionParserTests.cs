using Iceberg.CommandLine.Commands.Load.Options;
using Iceberg.CommandLine.Commands.Load.Services;
using Moq;
using System.Diagnostics.CodeAnalysis;

namespace Iceberg.CommandLine.Tests.Commands.Load.Options;

[ExcludeFromCodeCoverage]
public class PathOptionParserTests
{
    [Fact]
    public void Parse_SingleSolution()
    {
        // Arrange
        var paths = new[] { "solution.sln" };

        var mockFileService = new Mock<IFileService>();
        mockFileService.Setup(x => x.Exists(It.IsAny<string>()))
            .Returns(true);

        var pathOptionParser = new PathOptionParser(mockFileService.Object);

        // Act
        var parseResult = pathOptionParser.Parse(paths);

        // Assert
        Assert.False(parseResult.HasError);
        Assert.NotNull(parseResult.Value);
        Assert.Single(parseResult.Value, x => x == paths.First());
    }

    [Fact]
    public void Parse_SingleProject()
    {
        // Arrange
        var paths = new[] { "project.csproj" };

        var mockFileService = new Mock<IFileService>();
        mockFileService.Setup(x => x.Exists(It.IsAny<string>()))
            .Returns(true);

        var pathOptionParser = new PathOptionParser(mockFileService.Object);

        // Act
        var parseResult = pathOptionParser.Parse(paths);

        // Assert
        Assert.False(parseResult.HasError);
        Assert.NotNull(parseResult.Value);
        Assert.Single(parseResult.Value, x => x == paths.First());
    }

    [Fact]
    public void Parse_MultipleProjects()
    {
        // Arrange
        var paths = new[] { "project1.csproj", "project2.csproj" };

        var mockFileService = new Mock<IFileService>();
        mockFileService.Setup(x => x.Exists(It.IsAny<string>()))
            .Returns(true);

        var pathOptionParser = new PathOptionParser(mockFileService.Object);

        // Act
        var parseResult = pathOptionParser.Parse(paths);

        // Assert
        Assert.False(parseResult.HasError);
        Assert.NotNull(parseResult.Value);
        Assert.True(paths.SequenceEqual(parseResult.Value!));
    }

    [Fact]
    public void Parse_MultipleSolutions_Fails()
    {
        // Arrange
        var paths = new[] { "solution1.sln", "solution2.sln" };

        var mockFileService = new Mock<IFileService>();
        mockFileService.Setup(x => x.Exists(It.IsAny<string>()))
            .Returns(true);

        var pathOptionParser = new PathOptionParser(mockFileService.Object);

        // Act
        var parseResult = pathOptionParser.Parse(paths);

        // Assert
        Assert.True(parseResult.HasError);
        Assert.Equal("Only a single solution (.sln) may be provided.", parseResult.ErrorMessage);
    }

    [Fact]
    public void Parse_InvalidFileExtension_Fails()
    {
        // Arrange
        var paths = new[] { "project1.invalid" };

        var mockFileService = new Mock<IFileService>();
        mockFileService.Setup(x => x.Exists(It.IsAny<string>()))
            .Returns(true);

        var pathOptionParser = new PathOptionParser(mockFileService.Object);

        // Act
        var parseResult = pathOptionParser.Parse(paths);

        // Assert
        Assert.True(parseResult.HasError);
        Assert.Equal("Only the following file extensions are supported: .csproj, .sln", parseResult.ErrorMessage);
    }

    [Fact]
    public void Parse_FilesNotFound_Fails()
    {
        // Arrange
        var filePathThatExists = "project1.csproj";
        var filePathThatDoesNotExist = "project2.csproj";

        var mockFileService = new Mock<IFileService>();
        mockFileService.Setup(x => x.Exists(It.IsAny<string>()))
            .Returns((string filePath) => filePath == filePathThatExists);

        var pathOptionParser = new PathOptionParser(mockFileService.Object);

        // Act
        var parseResult = pathOptionParser.Parse(new[] { filePathThatExists, filePathThatDoesNotExist });

        // Assert
        Assert.True(parseResult.HasError);
        Assert.Equal($"The following files were not found: {filePathThatDoesNotExist}", parseResult.ErrorMessage);
    }
}
