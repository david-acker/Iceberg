using Iceberg.Map.DependencyMapper.Filters.Projects;
using System.Diagnostics.CodeAnalysis;

namespace Iceberg.Map.DependencyMapper.UnitTests.Filters.Projects;

[ExcludeFromCodeCoverage]
public class ProjectNameContainsFilterTests
{
    [Fact]
    public void Filter_Exclude_DefaultComparer()
    {
        // Arrange
        var textToExclude = "Test";
        var filter = new ProjectNameContainsFilter(
            textToExclude,
            ProjectNameContainsFilter.ProjectNameContainsFilterType.Exclude);

        var allProjectNames = new[] { "ProjectA.Test", "ProjectB.test" };
        var mockProjectWrappers = allProjectNames
            .Select(name => TestUtilities.GetMockProjectWrapper(name)).ToList();
        var projectWrappers = mockProjectWrappers.Select(x => x.Object);

        // Act
        var results = filter.Filter(projectWrappers);

        // Assert
        Assert.Single(results, x => x.Name == "ProjectB.test");
    }

    [Fact]
    public void Filter_Exclude_CaseInsensitive()
    {
        // Arrange
        var textToExclude = "Test";
        var filter = new ProjectNameContainsFilter(
            textToExclude,
            ProjectNameContainsFilter.ProjectNameContainsFilterType.Exclude,
            StringComparison.CurrentCultureIgnoreCase);

        var allProjectNames = new[] { "ProjectA.Test", "ProjectB.test" };
        var mockProjectWrappers = allProjectNames
            .Select(name => TestUtilities.GetMockProjectWrapper(name)).ToList();
        var projectWrappers = mockProjectWrappers.Select(x => x.Object);

        // Act
        var results = filter.Filter(projectWrappers);

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void Filter_Include_DefaultComparer()
    {
        // Arrange
        var textToInclude = "Test";
        var filter = new ProjectNameContainsFilter(
            textToInclude,
            ProjectNameContainsFilter.ProjectNameContainsFilterType.Include);

        var allProjectNames = new[] { "ProjectA.Test", "ProjectB.test" };
        var mockProjectWrappers = allProjectNames
            .Select(name => TestUtilities.GetMockProjectWrapper(name)).ToList();
        var projectWrappers = mockProjectWrappers.Select(x => x.Object);

        // Act
        var results = filter.Filter(projectWrappers);

        // Assert
        Assert.Single(results, x => x.Name == "ProjectA.Test");
    }

    [Fact]
    public void Filter_Include_CaseInsensitive()
    {
        // Arrange
        var textToInclude = "Test";
        var filter = new ProjectNameContainsFilter(
            textToInclude,
            ProjectNameContainsFilter.ProjectNameContainsFilterType.Include,
            StringComparison.CurrentCultureIgnoreCase);

        var allProjectNames = new[] { "ProjectA.Test", "ProjectB.test" };
        var mockProjectWrappers = allProjectNames
            .Select(name => TestUtilities.GetMockProjectWrapper(name)).ToList();
        var projectWrappers = mockProjectWrappers.Select(x => x.Object);

        // Act
        var results = filter.Filter(projectWrappers);

        // Assert
        Assert.Equal(2, results.Count());
        Assert.Contains(results, x => x.Name == "ProjectA.Test");
        Assert.Contains(results, x => x.Name == "ProjectB.test");
    }

    [Theory]
    [InlineData("ProjectA.Test", false)]
    [InlineData("ProjectB.test", true)]
    public void Predicate_Exclude_DefaultComparer(string projectName, bool expected)
    {
        // Arrange
        var textToExclude = "Test";
        var filter = new ProjectNameContainsFilter(
            textToExclude,
            ProjectNameContainsFilter.ProjectNameContainsFilterType.Exclude);

        var mockProjectWrapper = TestUtilities.GetMockProjectWrapper(projectName);
        var projectWrapper = mockProjectWrapper.Object;

        // Act
        var actual = filter.Predicate(projectWrapper);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("ProjectA.Test", false)]
    [InlineData("ProjectB.test", false)]
    public void Predicate_Exclude_CaseInsensitive(string projectName, bool expected)
    {
        // Arrange
        var textToExclude = "Test";
        var filter = new ProjectNameContainsFilter(
           textToExclude,
            ProjectNameContainsFilter.ProjectNameContainsFilterType.Exclude,
            StringComparison.CurrentCultureIgnoreCase);

        var mockProjectWrapper = TestUtilities.GetMockProjectWrapper(projectName);
        var projectWrapper = mockProjectWrapper.Object;

        // Act
        var actual = filter.Predicate(projectWrapper);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("ProjectA.Test", true)]
    [InlineData("ProjectB.test", false)]
    public void Predicate_Include_DefaultComparer(string projectName, bool expected)
    {
        // Arrange
        var textToInclude = "Test";
        var filter = new ProjectNameContainsFilter(
            textToInclude,
            ProjectNameContainsFilter.ProjectNameContainsFilterType.Include);

        var mockProjectWrapper = TestUtilities.GetMockProjectWrapper(projectName);
        var projectWrappers = mockProjectWrapper.Object;

        // Act
        var actual = filter.Predicate(projectWrappers);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("ProjectA.Test", true)]
    [InlineData("ProjectB.test", true)]
    public void Predicate_Include_CaseInsensitive(string projectName, bool expected)
    {
        // Arrange
        var textToInclude = "Test";
        var filter = new ProjectNameContainsFilter(
            textToInclude,
            ProjectNameContainsFilter.ProjectNameContainsFilterType.Include,
            StringComparison.CurrentCultureIgnoreCase);

        var mockProjectWrapper = TestUtilities.GetMockProjectWrapper(projectName);
        var projectWrapper = mockProjectWrapper.Object;

        // Act
        var actual = filter.Predicate(projectWrapper);

        // Assert
        Assert.Equal(expected, actual);
    }
}
