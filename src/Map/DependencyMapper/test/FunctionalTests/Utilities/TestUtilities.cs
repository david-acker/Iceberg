using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;

namespace Iceberg.Map.DependencyMapper.FunctionalTests.Utilities;

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
}
