using Iceberg.Map.DependencyMapper.Wrappers;

namespace Iceberg.Map.DependencyMapper.Filters.Projects;

/// <summary>
/// The default <see cref="IProjectFilter"/> filter. Applies no filtering logic.
/// </summary>
public class DefaultProjectFilter : IProjectFilter
{
    /// <summary>
    /// Returns the provided <see cref="IProjectWrapper"/> instances with no filtering logic applied.
    /// </summary>
    /// <param name="projectWrappers">The provided <see cref="IProjectWrapper"/> instances.</param>
    /// <returns>The provided <see cref="IProjectWrapper"/> instances.</returns>
    public IEnumerable<IProjectWrapper> Filter(IEnumerable<IProjectWrapper> projectWrappers) => projectWrappers;

    /// <summary>
    /// Returns the result of the filtering predicate for an individual <see cref="IProjectWrapper"/>.
    /// Always returns <see cref="true"/> to return all input items, thus applying no filtering logic. 
    /// </summary>
    /// <param name="projectWrapper">The provided <see cref="IProjectWrapper"/> instance.</param>
    /// <returns>Returns <see cref="true"/>.</returns>
    public bool Predicate(IProjectWrapper projectWrapper) => true;
}
