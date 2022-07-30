using Iceberg.Map.DependencyMapper.Wrappers;

namespace Iceberg.Map.DependencyMapper.Filters.Projects;

/// <summary>
/// An <see cref="IFilter{T}"/> used for filtering <see cref="IProjectWrapper"/> instances.
/// </summary>
public interface IProjectFilter : IFilter<IProjectWrapper>
{
}