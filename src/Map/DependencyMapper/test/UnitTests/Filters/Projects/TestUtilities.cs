using Iceberg.Map.DependencyMapper.Wrappers;
using Moq;
using System.Diagnostics.CodeAnalysis;

namespace Iceberg.Map.DependencyMapper.UnitTests.Filters.Projects;

[ExcludeFromCodeCoverage]
public static class TestUtilities
{
    public static Mock<IProjectWrapper> GetMockProjectWrapper(string projectName)
    {
        var mockProjectWrapper = new Mock<IProjectWrapper>();

        mockProjectWrapper.SetupGet(x => x.Name)
            .Returns(projectName);

        return mockProjectWrapper;
    }
}
