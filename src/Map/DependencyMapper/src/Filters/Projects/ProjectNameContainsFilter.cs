using Iceberg.Map.DependencyMapper.Wrappers;

namespace Iceberg.Map.DependencyMapper.Filters.Projects;

/// <summary>
/// An <see cref="IProjectFilter"/> which filters based on the <see cref="IProjectWrapper.Name"/>. 
/// </summary>
public class ProjectNameContainsFilter : IProjectFilter
{
    private readonly string _value;
    private readonly ProjectNameContainsFilterType _filterType;
    private readonly StringComparison _stringComparison;

    /// <summary>
    /// Creates and instance of <see cref="ProjectNameContainsFilter"/>.
    /// </summary>
    /// <param name="value">The value to check for in the project name.</param>
    /// <param name="filterType">The <see cref="ProjectNameContainsFilterType"/> used to specify the filtering behavior.</param>
    /// <param name="stringComparison">An optional <see cref="StringComparison"/>. Defaults to <see cref="StringComparison.CurrentCulture"/>.</param>
    public ProjectNameContainsFilter(
        string value,
        ProjectNameContainsFilterType filterType,
        StringComparison stringComparison = StringComparison.CurrentCulture)
    {
        _value = value;
        _filterType = filterType;
        _stringComparison = stringComparison;
    }

    /// <summary>
    /// Returns the filtered <see cref="IProjectWrapper"/> instances.
    /// </summary>
    /// <param name="projectWrappers">The input <see cref="IProjectWrapper"/> instances.</param>
    /// <returns>The filtered <see cref="IProjectWrapper"/> instances.</returns>
    public IEnumerable<IProjectWrapper> Filter(IEnumerable<IProjectWrapper> projectWrappers)
    {
        return projectWrappers.Where(p => Predicate(p));
    }

    /// <summary>
    /// Returns the result of the filtering predicate for the provided <see cref="IProjectWrapper"/> instance.
    /// </summary>
    /// <param name="projectWrapper">The <see cref="IProjectWrapper"/> instance.</param>
    /// <returns>The <see cref="bool"/> result of the filtering predicate.</returns>
    public bool Predicate(IProjectWrapper projectWrapper)
    {
        return _filterType switch
        {
            ProjectNameContainsFilterType.Exclude => !projectWrapper.Name.Contains(_value, _stringComparison),
            ProjectNameContainsFilterType.Include => projectWrapper.Name.Contains(_value, _stringComparison),
            _ => true
        };
    }

    /// <summary>
    /// The filtering behavior to use with the provided project names.
    /// </summary>
    public enum ProjectNameContainsFilterType
    {
        /// <summary>
        /// Exclude projects with names that contain the specified text.
        /// </summary>
        Exclude,

        /// <summary>
        /// Include projects with names that contain the specified text.
        /// </summary>
        Include
    }
}
