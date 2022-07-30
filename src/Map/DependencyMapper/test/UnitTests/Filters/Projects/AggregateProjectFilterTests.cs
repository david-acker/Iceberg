using Iceberg.Map.DependencyMapper.Filters.Projects;
using Iceberg.Map.DependencyMapper.Wrappers;
using Moq;
using System.Diagnostics.CodeAnalysis;

namespace Iceberg.Map.DependencyMapper.UnitTests.Filters.Projects;

[ExcludeFromCodeCoverage]
public class AggregateProjectFilterTests
{
    public static IEnumerable<object[]> GetPredicateFilterTestData()
    {
        yield return new object[] { new bool[] { true } };
        yield return new object[] { new bool[] { false } };

        yield return new object[] { new bool[] { true, true } };
        yield return new object[] { new bool[] { true, false } };
        yield return new object[] { new bool[] { false, false } };
        yield return new object[] { new bool[] { false, true } };
    }

    [Theory]
    [MemberData(nameof(GetPredicateFilterTestData))]
    public void Filter(bool[] predicateResults)
    {
        // Arrange
        var mockProjectFilters = GetMockProjectFilters(predicateResults);
        var projectFilters = mockProjectFilters.Select(x => x.Object);

        var projectWrapper = new Mock<IProjectWrapper>().Object;

        var aggregateProjectFilter = new AggregateProjectFilter(projectFilters);

        var expectedResultCount = predicateResults.All(x => x) ? 1 : 0;

        // Act
        var results = aggregateProjectFilter.Filter(new[] { projectWrapper });

        // Assert
        Assert.Equal(expectedResultCount, results.Count());
    }

    [Theory]
    [MemberData(nameof(GetPredicateFilterTestData))]
    public void Predicate(bool[] predicateResults)
    {
        // Arrange
        var mockProjectFilters = GetMockProjectFilters(predicateResults);
        var projectFilters = mockProjectFilters.Select(x => x.Object);

        var projectWrapper = new Mock<IProjectWrapper>().Object;

        var aggregateProjectFilter = new AggregateProjectFilter(projectFilters);

        var expected = predicateResults.All(x => x);

        // Act
        var actual = aggregateProjectFilter.Predicate(projectWrapper);

        // Assert
        Assert.Equal(expected, actual);
    }

    private static IEnumerable<Mock<IProjectFilter>> GetMockProjectFilters(bool[] predicateResults)
    {
        return predicateResults.Select(result =>
        {
            var mock = new Mock<IProjectFilter>();

            mock.Setup(x => x.Predicate(It.IsAny<IProjectWrapper>()))
                .Returns(result);

            return mock;
        })
        .ToList();
    }
}
