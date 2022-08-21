using Iceberg.CommandLine.Commands.View;
using Iceberg.CommandLine.Commands.View.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Diagnostics.CodeAnalysis;

namespace Iceberg.CommandLine.Tests.Commands.View;

[ExcludeFromCodeCoverage]
public class ViewCommandHandlerTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Handle(bool hasGeneratedMap)
    {
        // Arrange
        var mapName = hasGeneratedMap ? "generated-map.dgml" : null;

        var mockSessionContext = new Mock<IIcebergSessionContext>();
        mockSessionContext.SetupGet(x => x.LastGeneratedDependencyMap)
            .Returns(mapName);

        var mockProcessService = new Mock<IProcessService>();

        var handler = new ViewCommandHandler(
            mockSessionContext.Object, 
            mockProcessService.Object,
            NullLogger<ViewCommandHandler>.Instance);

        // Act
        handler.Handle();

        // Assert
        if (hasGeneratedMap)
        {
            mockProcessService.Verify(x => x.OpenFileWithDefaultProgram(It.IsAny<string>()), Times.Once);
            mockProcessService.Verify(x => x.OpenFileWithDefaultProgram(It.Is<string>(y => y == mapName)), Times.Once);
        }
        else
        {
            mockProcessService.Verify(x => x.OpenFileWithDefaultProgram(It.IsAny<string>()), Times.Never);
        }
    }
}
