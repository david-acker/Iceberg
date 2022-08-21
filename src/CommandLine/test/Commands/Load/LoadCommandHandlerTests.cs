using Iceberg.CommandLine.Commands.Load;
using Moq;
using System.Diagnostics.CodeAnalysis;

namespace Iceberg.CommandLine.Tests.Commands.Load;

[ExcludeFromCodeCoverage]
public class LoadCommandHandlerTests
{
    [Fact]
    public async Task Handle_SinglePath_LoadsAsSolution()
    {
        // Arrange
        var paths = new[] { "path" };

        var mockSessionContext = new Mock<IIcebergSessionContext>();

        var handler = new LoadCommandHandler(mockSessionContext.Object);

        // Act
        await handler.Handle(paths);

        // Assert
        mockSessionContext.Verify(x => x.LoadSolution(It.IsAny<string>()), Times.Once);
        mockSessionContext.Verify(x => x.LoadSolution(It.Is<string>(y => y == paths.First())), Times.Once);

        mockSessionContext.Verify(x => x.LoadProjects(It.IsAny<IEnumerable<string>>()), Times.Never);
    }

    [Fact]
    public async Task Handle_MultiplePaths_LoadsAsProjects()
    {
        // Arrange
        var paths = new[] { "path1", "path2" };

        var mockSessionContext = new Mock<IIcebergSessionContext>();

        var handler = new LoadCommandHandler(mockSessionContext.Object);

        // Act
        await handler.Handle(paths);

        // Assert
        mockSessionContext.Verify(x => x.LoadSolution(It.IsAny<string>()), Times.Never);

        mockSessionContext.Verify(x => x.LoadProjects(It.IsAny<IEnumerable<string>>()), Times.Once);
        mockSessionContext.Verify(x => x.LoadProjects(It.Is<IEnumerable<string>>(y => y.SequenceEqual(paths))), Times.Once);
    }
}
