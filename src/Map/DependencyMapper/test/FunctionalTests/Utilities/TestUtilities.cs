using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;
using Iceberg.Map.DependencyMapper.Context;
using Iceberg.Map.DependencyMapper.Selectors;
using Iceberg.Map.DependencyMapper.Wrappers;
using Microsoft.Extensions.Logging.Abstractions;
using Iceberg.Map.Metadata;

namespace Iceberg.Map.DependencyMapper.FunctionalTests.Utilities;

[ExcludeFromCodeCoverage]
public static class TestUtilities
{
    public static ProjectInfo CreateProjectInfo(string projectName)
    {
        var projectId = ProjectId.CreateNewId();
        var versionStamp = VersionStamp.Create();

        return ProjectInfo.Create(
            projectId,
            versionStamp,
            projectName,
            projectName,
            LanguageNames.CSharp);
    }

    public static Workspace CreateWorkspace(IEnumerable<ProjectTemplate> projectTemplates)
    {
        var workspace = new AdhocWorkspace();

        foreach (var projectTemplate in projectTemplates)
        {
            var projectInfo = CreateProjectInfo(projectTemplate.ProjectName);
            var project = workspace.AddProject(projectInfo);

            foreach (var documentTemplate in projectTemplate.Documents)
            {
                var sourceText = SourceText.From(documentTemplate.Text);
                workspace.AddDocument(project.Id, documentTemplate.DocumentName, sourceText);
            }
        }

        return workspace;
    }

    public static MethodSolutionContext CreateMethodSolutionContext(Workspace workspace)
    {
        var symbolEqualityComparer = new SymbolEqualityComparerWrapper();
        var symbolFinder = new SymbolFinderWrapper();

        var methodSelectors = new IMethodSelector[]
        {
            new AbstractOrVirtualMethodSelector(symbolEqualityComparer, symbolFinder),
            new ConcreteMethodSelector(),
            new OverriddenMethodSelector(symbolEqualityComparer)
        };

        var solutionWrapper = new SolutionWrapper(workspace.CurrentSolution);

        return new MethodSolutionContext(
            NullLoggerFactory.Instance,
            methodSelectors,
            solutionWrapper,
            symbolEqualityComparer,
            symbolFinder);
    }

    public static void AssertGeneratedMethodDependencyMap(MethodDependencyMap expected, MethodDependencyMap actual)
    {
        // TODO: Add more assertions and return better/more applicable map data on failed assertions

        Assert.Equal(expected.Count, actual.Count);

        Assert.True(expected.Keys.Select(x => x.DisplayName).ToHashSet()
            .SetEquals(actual.Keys.Select(x => x.DisplayName).ToHashSet()));

        foreach (var key in expected.Keys)
        {
            var expectedValue = expected[key];
            var actualValue = actual[key];

            Assert.Equal(expectedValue.Count, actualValue.Count);

            Assert.True(expectedValue.Select(x => x.DisplayName).ToHashSet()
                .SetEquals(actualValue.Select(x => x.DisplayName).ToHashSet()));

            Assert.Equal(expectedValue.Select(x => x.SourcePath).OrderBy(x => x),
                actualValue.Select(x => x.SourcePath).OrderBy(x => x));
        }
    }
}
