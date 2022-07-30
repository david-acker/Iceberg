using Microsoft.CodeAnalysis;

namespace Iceberg.Map.DependencyMapper.Wrappers;

public interface IProjectWrapper
{
    /// <summary>
    /// The underlying <see cref="Microsoft.CodeAnalysis.Project"/> instance.
    /// </summary>
    Project Project { get; }

    /// <summary>
    /// The <see cref="Microsoft.CodeAnalysis.Project.Name"/> of the underlying instance.
    /// </summary>
    string Name { get; }
}

/// <summary>
/// Wrapper class for <see cref="Microsoft.CodeAnalysis.Project"/>.
/// </summary>
public class ProjectWrapper : IProjectWrapper
{
    /// <inheritdoc/>
    public Project Project { get; }

    /// <inheritdoc/>
    public string Name => Project.Name;

    /// <summary>
    /// Creates an instance of <see cref="ProjectWrapper"/>.
    /// </summary>
    /// <param name="project">The <see cref="Microsoft.CodeAnalysis.Project"/> instance to wrap.</param>
    public ProjectWrapper(Project project)
    {
        Project = project;
    }
}
