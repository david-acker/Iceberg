using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Extensions.Logging;

namespace Iceberg.CommandLine;

internal interface IIcebergSessionContext
{
    Solution? Solution { get; }
    bool IsSolutionLoaded { get; }
    Task LoadSolution(string solutionPath);
    Task LoadProjects(IEnumerable<string> projectPaths);
    void UnloadSolution();

    string? LastGeneratedDependencyMap { get; set; }
}

internal sealed partial class IcebergSessionContext : IIcebergSessionContext
{
    private readonly ILogger<IcebergSessionContext> _logger;
    private readonly MSBuildWorkspace _workspace;

    public Solution? Solution { get; private set; } = null;
    public bool IsSolutionLoaded => Solution is not null;

    public string? LastGeneratedDependencyMap { get; set; }

    public IcebergSessionContext(ILogger<IcebergSessionContext> logger)
    {
        // TODO: Move this into some sort of workspace-specific infrastructure/helper service.
        if (!MSBuildLocator.IsRegistered)
        {
            var instances = MSBuildLocator.QueryVisualStudioInstances().ToArray();
            MSBuildLocator.RegisterInstance(instances.OrderByDescending(x => x.Version).First());
        }

        _workspace = MSBuildWorkspace.Create();
        _logger = logger;
    }

    public async Task LoadSolution(string solutionPath)
    {
        await Load(async () =>
        {
            Log.LoadingSolution(_logger, solutionPath);

            var solution = await _workspace.OpenSolutionAsync(solutionPath);

            Log.LoadedSolution(_logger, solutionPath);

            return solution;
        });
    }

    public async Task LoadProjects(IEnumerable<string> projectPaths)
    {
        await Load(async () =>
        {
            foreach (var projectPath in projectPaths)
            {
                Log.LoadingProject(_logger, projectPath);
                
                await _workspace.OpenProjectAsync(projectPath);

                Log.LoadedProject(_logger, projectPath);
            }

            return _workspace.CurrentSolution;
        });
    }

    private async Task Load(Func<Task<Solution>> solutionDelegate)
    {
        UnloadSolution();

        Solution = await solutionDelegate();
    }

    public void UnloadSolution()
    {
        if (IsSolutionLoaded)
        {
            Log.UnloadingSolution(_logger);

            Solution = null;
            _workspace.CloseSolution();

            Log.UnloadedSolution(_logger);
        }
        else
        {
            Log.NoSolutionLoaded(_logger);
        }
    }

    private static partial class Log
    {
        [LoggerMessage(1, LogLevel.Information, "Unloading the current solution.", EventName = "UnloadingSolution")]
        public static partial void UnloadingSolution(ILogger logger);

        [LoggerMessage(2, LogLevel.Information, "Successfully unloaded the current solution.", EventName = "UnloadedSolution")]
        public static partial void UnloadedSolution(ILogger logger);

        [LoggerMessage(3, LogLevel.Information, "No solution is currently loaded.", EventName = "NoSolutionLoaded")]
        public static partial void NoSolutionLoaded(ILogger logger);

        [LoggerMessage(4, LogLevel.Information, "Loading solution: {SolutionPath}", EventName = "LoadingSolution")]
        public static partial void LoadingSolution(ILogger logger, string solutionPath);

        [LoggerMessage(5, LogLevel.Information, "Loaded solution: {SolutionPath}", EventName = "LoadedSolution")]
        public static partial void LoadedSolution(ILogger logger, string solutionPath);

        [LoggerMessage(6, LogLevel.Information, "Loading project: {ProjectPath}", EventName = "LoadingProject")]
        public static partial void LoadingProject(ILogger logger, string projectPath);

        [LoggerMessage(7, LogLevel.Information, "Loaded project: {ProjectPath}", EventName = "LoadedProject")]
        public static partial void LoadedProject(ILogger logger, string projectPath);
    }
}
