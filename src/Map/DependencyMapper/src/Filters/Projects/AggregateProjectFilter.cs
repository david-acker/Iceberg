using Iceberg.Map.DependencyMapper.Wrappers;

namespace Iceberg.Map.DependencyMapper.Filters.Projects;

/// <summary>
/// Aggregates <see cref="IProjectFilter"/> combining multiple <see cref="IProjectFilter"/> instances.
/// </summary>
public class AggregateProjectFilter : IProjectFilter
{
    private readonly IEnumerable<IProjectFilter> _projectFilters;

    /// <summary>
    /// Creates an <see cref="AggregateProjectFilter"/> instance.
    /// </summary>
    /// <param name="projectFilters">The <see cref="IProjectFilter"/> instances to aggregate.</param>
    public AggregateProjectFilter(IEnumerable<IProjectFilter> projectFilters)
    {
        _projectFilters = projectFilters;
    }
    /// <summary>
    /// Returns the filtered <see cref="IProjectWrapper"/> instances.
    /// </summary>
    /// <param name="projectWrappers">The input <see cref="IProjectWrapper"/> instances.</param>
    /// <returns>The filtered <see cref="IProjectWrapper"/> instances.</returns>
    public IEnumerable<IProjectWrapper> Filter(IEnumerable<IProjectWrapper> projectWrappers)
    {
        return projectWrappers.Where(project => _projectFilters.All(filter => filter.Predicate(project)));
    }

    /// <summary>
    /// Returns the aggregate result of the filtering predicates for the provided <see cref="IProjectWrapper"/> instance.
    /// </summary>
    /// <param name="projectWrapper">The <see cref="IProjectWrapper"/> instance.</param>
    /// <returns>The aggregate <see cref="bool"/> result of the filtering predicates.</returns>
    public bool Predicate(IProjectWrapper projectWrapper)
    {
        return _projectFilters.All(filter => filter.Predicate(projectWrapper));
    }
}
