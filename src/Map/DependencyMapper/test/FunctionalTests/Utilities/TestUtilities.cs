using Iceberg.Map.DependencyMapper.Context;
using Iceberg.Map.DependencyMapper.Selectors;
using Iceberg.Map.DependencyMapper.Wrappers;
using Iceberg.Map.Metadata;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Logging.Abstractions;
using System.Diagnostics.CodeAnalysis;

namespace Iceberg.Map.DependencyMapper.FunctionalTests.Utilities;

[ExcludeFromCodeCoverage]
internal static class TestUtilities
{
    public static async Task<MethodSolutionContext> SetupMethodSolutionContext(ProjectTemplate projectTemplate) =>
        await SetupMethodSolutionContext(new[] { projectTemplate });

    public static async Task<MethodSolutionContext> SetupMethodSolutionContext(ProjectTemplate[] projectTemplates)
    {
        var workspace = CreateWorkspace(projectTemplates);

        var compilations = await Task.WhenAll(
            workspace.CurrentSolution.Projects.Select(x => x.GetCompilationAsync()));

        foreach (var compilation in compilations)
        {
            Assert.DoesNotContain(compilation!.GetDiagnostics(),
                diagnostic => diagnostic.Severity == DiagnosticSeverity.Error || diagnostic.Severity == DiagnosticSeverity.Warning);
        }

        return CreateMethodSolutionContext(workspace);
    }

    private static Workspace CreateWorkspace(IEnumerable<ProjectTemplate> projectTemplates)
    {
        var workspace = new AdhocWorkspace();

        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => !assembly.IsDynamic)
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            .Cast<MetadataReference>();

        foreach (var projectTemplate in projectTemplates)
        {
            var projectInfo = CreateProjectInfo(projectTemplate.ProjectName, references);
            var project = workspace.AddProject(projectInfo);

            foreach (var documentTemplate in projectTemplate.Documents)
            {
                var sourceText = SourceText.From(documentTemplate.Text);
                workspace.AddDocument(project.Id, documentTemplate.DocumentName, sourceText);
            }
        }

        return workspace;
    }

    private static ProjectInfo CreateProjectInfo(string projectName, IEnumerable<MetadataReference> references)
    {
        var projectId = ProjectId.CreateNewId();
        var versionStamp = VersionStamp.Create();

        return ProjectInfo.Create(
            projectId,
            versionStamp,
            projectName,
            projectName,
            LanguageNames.CSharp,
            metadataReferences: references,
            compilationOptions: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
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

        var expectedDisplayNames = expected.Keys.Select(x => x.DisplayName).ToHashSet();
        var actualDisplayNames = actual.Keys.Select(x => x.DisplayName).ToHashSet();

        Assert.True(expectedDisplayNames.SetEquals(actualDisplayNames));

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
