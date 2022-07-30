using Iceberg.Map.DependencyMapper.Filters.Projects;
using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;

namespace Iceberg.Map.DependencyMapper.UnitTests.Filters.Projects;

[ExcludeFromCodeCoverage]
public class DefaultProjectFilterTests
{
    [Fact]
    public void Filter_ReturnsUnfilteredProjects()
    {
        // Arrange
        var filter = new DefaultProjectFilter();

        var mockProjectWrappers = Enumerable.Range(1, 3)
            .Select(i => TestUtilities.GetMockProjectWrapper(i.ToString())).ToList();
        var projectWrappers = mockProjectWrappers.Select(x => x.Object);

        // Act
        var results = filter.Filter(projectWrappers);

        // Assert
        Assert.Equal(projectWrappers, results);
    }

    [Fact]
    public void Predicate_AlwaysReturnsTrue()
    {
        // Arrange
        var filter = new DefaultProjectFilter();

        var mockProjectWrapper = TestUtilities.GetMockProjectWrapper(string.Empty);
        var projectWrapper = mockProjectWrapper.Object;

        // Act
        var result = filter.Predicate(projectWrapper);

        // Assert
        Assert.True(result);
    }
}
