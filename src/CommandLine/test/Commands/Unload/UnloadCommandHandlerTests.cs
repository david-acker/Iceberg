using Iceberg.CommandLine.Commands.Unload;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Diagnostics.CodeAnalysis;

namespace Iceberg.CommandLine.Tests.Commands.Unload;

[ExcludeFromCodeCoverage]
public class UnloadCommandHandlerTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Handle(bool isSolutionLoaded)
    {
        // Arrange
        var mockSessionContext = new Mock<IIcebergSessionContext>();
        mockSessionContext.SetupGet(x => x.IsSolutionLoaded)
            .Returns(isSolutionLoaded);

        var handler = new UnloadCommandHandler(
            mockSessionContext.Object,
            NullLogger<UnloadCommandHandler>.Instance);

        // Act
        handler.Handle();

        // Assert
        mockSessionContext.Verify(x => x.UnloadSolution(), isSolutionLoaded ? Times.Once : Times.Never);
    }
}
