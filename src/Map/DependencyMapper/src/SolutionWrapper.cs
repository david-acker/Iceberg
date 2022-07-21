using Microsoft.CodeAnalysis;

namespace Iceberg.Map.DependencyMapper;

public interface ISolutionWrapper
{
    Solution Solution { get; init; }

    IEnumerable<Project> Projects { get; }
}

public class SolutionWrapper : ISolutionWrapper
{
    public Solution Solution { get; init; }

    public IEnumerable<Project> Projects => Solution.Projects;

    public SolutionWrapper(Solution solution)
    {
       Solution = solution;
    }
}
