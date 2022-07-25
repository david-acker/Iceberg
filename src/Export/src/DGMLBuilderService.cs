using Iceberg.Map.Metadata;
using System.Xml.Linq;

namespace Iceberg.Export;

public interface IDGMLBuilderService
{
    public XElement ExportDependencyMap(
        string entryPointName,
        MethodDependencyMap dependencyMap,
        int namespaceSpecificity = 2);
}

public class DGMLBuilderService : IDGMLBuilderService
{
    public XElement ExportDependencyMap(
        string entryPointName,
        MethodDependencyMap dependencyMap,
        int namespaceSpecificity = 2)
    {
        var builder = new DGMLBuilder($"{entryPointName} Dependency Map");

        var categoryNames = dependencyMap.Keys
            .Select(x => GetCategoryName(x, namespaceSpecificity))
            .Distinct();

        var colors = DGMLColorProvider.GetColors();
        var random = new Random();

        foreach (var categoryName in categoryNames)
        {
            var index = random.Next(colors.Count);

            var color = colors[index];
            colors.RemoveAt(index);

            builder.AddCategory(categoryName, color);
        }

        foreach (var entry in dependencyMap.Keys)
        {
            var elementId = GetElementName(entry);
            var categoryId = GetCategoryName(entry, namespaceSpecificity);

            builder.AddNode(elementId, categoryId, referencePath: entry.SourcePath);
        }

        foreach (var entry in dependencyMap)
        {
            var source = GetElementName(entry.Key);

            foreach (var dependency in entry.Value)
            {
                var target = GetElementName(dependency);

                builder.AddLink(source, target);
            }
        }

        return builder.Content;
    }

    private static string GetElementName(MethodMetadata metadata)
    {
        var className = metadata.ClassName;

        var classNameIndex = metadata.DisplayName.IndexOf(className);

        return metadata.DisplayName[classNameIndex..];
    }

    private static string GetCategoryName(MethodMetadata metadata, int namespaceSpecificity)
    {
        return string.Join('.', metadata.NamespaceComponents.Take(namespaceSpecificity));
    }
}