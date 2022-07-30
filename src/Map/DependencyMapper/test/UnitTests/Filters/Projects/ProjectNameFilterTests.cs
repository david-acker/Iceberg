using Iceberg.Map.DependencyMapper.Filters.Projects;
using System.Diagnostics.CodeAnalysis;

namespace Iceberg.Map.DependencyMapper.UnitTests.Filters.Projects;

[ExcludeFromCodeCoverage]
public class ProjectNameFilterTests
{
    [Fact]
    public void Filter_Exclude_DefaultComparer()
    {
        // Arrange
        var projectNamesToExclude = new[] { "ProjectName" };
        var filter = new ProjectNameFilter(
            projectNamesToExclude,
            ProjectNameFilter.ProjectNameFilterType.Exclude);

        var allProjectNames = new[] { "ProjectName", "projectname" };
        var mockProjectWrappers = allProjectNames
            .Select(name => TestUtilities.GetMockProjectWrapper(name)).ToList();
        var projectWrappers = mockProjectWrappers.Select(x => x.Object);

        // Act
        var results = filter.Filter(projectWrappers);

        // Assert
        Assert.Single(results, x => x.Name == "projectname");
    }

    [Fact]
    public void Filter_Exclude_CaseInsensitive()
    {
        // Arrange
        var projectNamesToExclude = new[] { "ProjectName" };
        var filter = new ProjectNameFilter(
            projectNamesToExclude,
            ProjectNameFilter.ProjectNameFilterType.Exclude,
            StringComparer.CurrentCultureIgnoreCase);

        var allProjectNames = new[] { "ProjectName", "projectname" };
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
        var projectNamesToInclude = new[] { "ProjectName" };
        var filter = new ProjectNameFilter(
            projectNamesToInclude,
            ProjectNameFilter.ProjectNameFilterType.Include);

        var allProjectNames = new[] { "ProjectName", "projectname" };
        var mockProjectWrappers = allProjectNames
            .Select(name => TestUtilities.GetMockProjectWrapper(name)).ToList();
        var projectWrappers = mockProjectWrappers.Select(x => x.Object);

        // Act
        var results = filter.Filter(projectWrappers);

        // Assert
        Assert.Single(results, x => x.Name == "ProjectName");
    }

    [Fact]
    public void Filter_Include_CaseInsensitive()
    {
        // Arrange
        var projectNamesToInclude = new[] { "ProjectName" };
        var filter = new ProjectNameFilter(
            projectNamesToInclude,
            ProjectNameFilter.ProjectNameFilterType.Include,
            StringComparer.CurrentCultureIgnoreCase); 

        var allProjectNames = new[] { "ProjectName", "projectname" };
        var mockProjectWrappers = allProjectNames
            .Select(name => TestUtilities.GetMockProjectWrapper(name)).ToList();
        var projectWrappers = mockProjectWrappers.Select(x => x.Object);

        // Act
        var results = filter.Filter(projectWrappers);

        // Assert
        Assert.Equal(2, results.Count());
        Assert.Contains(results, x => x.Name == "ProjectName");
        Assert.Contains(results, x => x.Name == "projectname");
    }

    [Theory]
    [InlineData("ProjectName", false)]
    [InlineData("projectname", true)]
    public void Predicate_Exclude_DefaultComparer(string projectName, bool expected)
    {
        // Arrange
        var projectNamesToExclude = new[] { "ProjectName" };
        var filter = new ProjectNameFilter(
            projectNamesToExclude,
            ProjectNameFilter.ProjectNameFilterType.Exclude);

        var mockProjectWrapper = TestUtilities.GetMockProjectWrapper(projectName);
        var projectWrapper = mockProjectWrapper.Object;

        // Act
        var actual = filter.Predicate(projectWrapper);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("ProjectName", false)]
    [InlineData("projectname", false)]
    public void Predicate_Exclude_CaseInsensitive(string projectName, bool expected)
    {
        // Arrange
        var projectNamesToExclude = new[] { "ProjectName" };
        var filter = new ProjectNameFilter(
            projectNamesToExclude,
            ProjectNameFilter.ProjectNameFilterType.Exclude,
            StringComparer.CurrentCultureIgnoreCase);

        var mockProjectWrapper = TestUtilities.GetMockProjectWrapper(projectName);
        var projectWrapper = mockProjectWrapper.Object;

        // Act
        var actual = filter.Predicate(projectWrapper);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("ProjectName", true)]
    [InlineData("projectname", false)]
    public void Predicate_Include_DefaultComparer(string projectName, bool expected)
    {
        // Arrange
        var projectNamesToInclude = new[] { "ProjectName" };
        var filter = new ProjectNameFilter(
            projectNamesToInclude,
            ProjectNameFilter.ProjectNameFilterType.Include);

        var mockProjectWrapper = TestUtilities.GetMockProjectWrapper(projectName);
        var projectWrappers = mockProjectWrapper.Object;

        // Act
        var actual = filter.Predicate(projectWrappers);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("ProjectName", true)]
    [InlineData("projectname", true)]
    public void Predicate_Include_CaseInsensitive(string projectName, bool expected)
    {
        // Arrange
        var projectNamesToInclude = new[] { "ProjectName" };
        var filter = new ProjectNameFilter(
            projectNamesToInclude,
            ProjectNameFilter.ProjectNameFilterType.Include,
            StringComparer.CurrentCultureIgnoreCase);

        var mockProjectWrapper = TestUtilities.GetMockProjectWrapper(projectName);
        var projectWrapper = mockProjectWrapper.Object;

        // Act
        var actual = filter.Predicate(projectWrapper);

        // Assert
        Assert.Equal(expected, actual);
    }
}
