using Microsoft.CodeAnalysis;

namespace Iceberg.Map.DependencyMapper.Wrappers;

public interface ISolutionWrapper
{
    /// <summary>
    /// The underlying <see cref="Microsoft.CodeAnalysis.Solution"/> instance.
    /// </summary>
    Solution Solution { get; }

    /// <summary>
    /// The <see cref="Project"/> instances contained in the <see cref="Microsoft.CodeAnalysis.Solution"/>
    /// wrapper in <see cref="ProjectWrapper"/>.
    /// </summary>
    IEnumerable<ProjectWrapper> Projects { get; }
}

/// <summary>
/// Wrapper class for <see cref="Microsoft.CodeAnalysis.Solution"/>.
/// </summary>
public class SolutionWrapper : ISolutionWrapper
{
    /// <inheritdoc />
    public Solution Solution { get; }

    /// <inheritdoc />
    public IEnumerable<ProjectWrapper> Projects { get; }

    /// <summary>
    /// Creates an instance of <see cref="SolutionWrapper"/>.
    /// </summary>
    /// <param name="solution">The <see cref="Microsoft.CodeAnalysis.Solution"/> instance to wrap.</param>
    public SolutionWrapper(Solution solution)
    {
        Solution = solution;
        Projects = solution.Projects.Select(p => new ProjectWrapper(p));
    }
}
