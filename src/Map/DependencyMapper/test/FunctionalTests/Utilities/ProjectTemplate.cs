using System.Diagnostics.CodeAnalysis;

namespace Iceberg.Map.DependencyMapper.FunctionalTests.Utilities;

[ExcludeFromCodeCoverage]
public class ProjectTemplate
{
    public string ProjectName { get; set; } = string.Empty;
    public IEnumerable<DocumentTemplate> Documents { get; set; } = new List<DocumentTemplate>();
}
