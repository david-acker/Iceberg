using Iceberg.Map.DependencyMapper.Wrappers;
using Microsoft.CodeAnalysis;

namespace Iceberg.Map.DependencyMapper.Filters.Projects;

/// <summary>
/// An <see cref="IProjectFilter"/> which filters based on the <see cref="IProjectWrapper.Name"/>.
/// </summary>
public class ProjectNameFilter : IProjectFilter
{
    private readonly HashSet<string> _projectNames;
    private readonly ProjectNameFilterType _filterType;

    /// <summary>
    /// Creates an instance of <see cref="ProjectNameFilter"/>.
    /// </summary>
    /// <param name="projectNames">An enumerable of project names to filter against. Corresponds to <see cref="IProjectWrapper.Name"/>.</param>
    /// <param name="filterType">The <see cref="ProjectNameFilterType"/> used to specify the filtering behavior.</param>
    /// <param name="stringComparer">An optional <see cref="StringComparer"/> instance. Defaults to <see cref="StringComparer.CurrentCulture"/>.</param>
    public ProjectNameFilter(
        IEnumerable<string> projectNames,
        ProjectNameFilterType filterType,
        StringComparer? stringComparer = null)
    {
        stringComparer ??= StringComparer.CurrentCulture;

        _projectNames = projectNames.ToHashSet(stringComparer);
        _filterType = filterType;
    }

    /// <summary>
    /// Returns the filtered <see cref="IProjectWrapper"/> instances.
    /// </summary>
    /// <param name="projectWrappers">The input <see cref="IProjectWrapper"/> instances.</param>
    /// <returns>The filtered <see cref="IProjectWrapper"/> instances.</returns>
    public IEnumerable<IProjectWrapper> Filter(IEnumerable<IProjectWrapper> projectWrappers)
    {
        return _filterType switch
        {
            ProjectNameFilterType.Exclude => projectWrappers.Where(p => !_projectNames.Contains(p.Name)),
            ProjectNameFilterType.Include => projectWrappers.Where(p => _projectNames.Contains(p.Name)),
            _ => projectWrappers
        };
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
            ProjectNameFilterType.Exclude => !_projectNames.Contains(projectWrapper.Name),
            ProjectNameFilterType.Include => _projectNames.Contains(projectWrapper.Name),
            _ => true
        };
    }

    /// <summary>
    /// The filtering behavior to use with the provided project names.
    /// </summary>
    public enum ProjectNameFilterType
    {
        /// <summary>
        /// Exclude projects with the specified names.
        /// </summary>
        Exclude,

        /// <summary>
        /// Include projects with the specified names.
        /// </summary>
        Include
    }
}
