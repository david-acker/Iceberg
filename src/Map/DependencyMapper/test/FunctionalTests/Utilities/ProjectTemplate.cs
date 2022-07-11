namespace Iceberg.Map.DependencyMapper.FunctionalTests.Utilities;

public class ProjectTemplate
{
    public string ProjectName { get; set; } = string.Empty;
    public IEnumerable<DocumentTemplate> Documents { get; set; } = new List<DocumentTemplate>();
}
