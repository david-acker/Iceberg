using Microsoft.CodeAnalysis;

namespace Iceberg.Map.DependencyMapper.Wrappers;

public interface ISolutionWrapper
{
    /// <summary>
    /// The underlying <see cref="Microsoft.CodeAnalysis.Solution"/>.
    /// </summary>
    Solution Solution { get; init; }

    /// <summary>
    /// The <see cref="Project"/> instances contained in the <see cref="Microsoft.CodeAnalysis.Solution"/>.
    /// </summary>
    IEnumerable<Project> Projects { get; }
}

/// <summary>
/// Wrapper class for <see cref="Microsoft.CodeAnalysis.Solution"/>.
/// </summary>
public class SolutionWrapper : ISolutionWrapper
{
    /// <inheritdoc />
    public Solution Solution { get; init; }

    /// <inheritdoc />
    public IEnumerable<Project> Projects => Solution.Projects;

    public SolutionWrapper(Solution solution)
    {
        Solution = solution;
    }
}
