using Iceberg.Map.Metadata;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;

namespace Iceberg.Export.IntegrationTests;

[ExcludeFromCodeCoverage]
public class DGMLBuilderServiceTests
{
    [Fact]
    public void ItWillExportDependencyMap()
    {
        // Arrange
        var entryPointName = "EntryClass.EntryMethod";
        var dependencyMap = new MethodDependencyMap
        {
            {
                new MethodMetadata("A.AA.EntryClass.EntryMethod()", "EntryClass.cs"),
                new HashSet<MethodMetadata>
            {
                new MethodMetadata("A.AA.EntryClass.PrivateMethod()", "EntryClass.cs"),
                new MethodMetadata("A.BB.ChildClass.ChildMethod()", "ChildClass.cs")
            }
            },

            {
                new MethodMetadata("A.AA.EntryClass.PrivateMethod()", "EntryClass.cs"),
                new HashSet<MethodMetadata>()
            },

            {
                new MethodMetadata("A.BB.ChildClass.ChildMethod()", "ChildClass.cs"),
                new HashSet<MethodMetadata>
            {
                new MethodMetadata("A.CC.GrandchildClass.GrandchildMethod()", "GrandchildClass.cs")
            }
            },

            {
                new MethodMetadata("A.CC.GrandchildClass.GrandchildMethod()", "GrandchildClass.cs"),
                new HashSet<MethodMetadata>()
            }
        };

        var expectedNodes = new[]
        {
            new Node { Id = "EntryClass.EntryMethod()", Label = "EntryClass.EntryMethod()", Category = "A.AA", Reference = "EntryClass.cs" },
            new Node { Id = "EntryClass.PrivateMethod()", Label = "EntryClass.PrivateMethod()", Category = "A.AA", Reference = "EntryClass.cs" },
            new Node { Id = "ChildClass.ChildMethod()", Label = "ChildClass.ChildMethod()", Category = "A.BB", Reference = "ChildClass.cs" },
            new Node { Id = "GrandchildClass.GrandchildMethod()", Label = "GrandchildClass.GrandchildMethod()", Category = "A.CC", Reference = "GrandchildClass.cs" }
        };

        var expectedLinks = new[]
        {
            new Link { Source = "EntryClass.EntryMethod()", Target = "EntryClass.PrivateMethod()" },
            new Link { Source = "EntryClass.EntryMethod()", Target = "ChildClass.ChildMethod()" },
            new Link { Source = "ChildClass.ChildMethod()", Target = "GrandchildClass.GrandchildMethod()" }
        };

        var expectedCategories = new[]
        {
            new Category { Id = "A.AA" },
            new Category { Id = "A.BB" },
            new Category { Id = "A.CC" }
        };

        var service = new DGMLBuilderService();

        // Act
        var generatedDGMLElement = service.ExportDependencyMap(entryPointName, dependencyMap);

        // Assert
        Assert.EndsWith("DirectedGraph", generatedDGMLElement.Name.ToString());
        Assert.Equal($"{entryPointName} Dependency Map", generatedDGMLElement.Attribute("Title")?.Value);

        Assert.Equal(3, generatedDGMLElement.Elements().Count());

        var nodesElement = Assert.Single(generatedDGMLElement.Elements(), x => x.Name.ToString().EndsWith("Nodes"));
        AssertNodes(expectedNodes, nodesElement.Elements());

        var linksElement = Assert.Single(generatedDGMLElement.Elements(), x => x.Name.ToString().EndsWith("Links"));
        AssertLinks(expectedLinks, linksElement.Elements());

        var categoriesElement = Assert.Single(generatedDGMLElement.Elements(), x => x.Name.ToString().EndsWith("Categories"));
        AssertCategories(expectedCategories, categoriesElement.Elements());
    }

    private static void AssertNodes(IEnumerable<Node> expectedNodes, IEnumerable<XElement> actualNodes)
    {
        Assert.Equal(expectedNodes.Count(), actualNodes.Count());

        foreach (var expectedNode in expectedNodes)
        {
            var actualNode = Assert.Single(actualNodes, x => x.Attribute("Id")?.Value == expectedNode.Id);

            Assert.EndsWith("Node", actualNode.Name.ToString());

            Assert.Equal(expectedNode.Label, actualNode.Attribute("Label")?.Value);
            Assert.Equal(expectedNode.Category, actualNode.Attribute("Category")?.Value);
            Assert.Equal(expectedNode.Reference, actualNode.Attribute("Reference")?.Value);
        }
    }

    private static void AssertLinks(IEnumerable<Link> expectedLinks, IEnumerable<XElement> actualLinks)
    {
        Assert.Equal(expectedLinks.Count(), actualLinks.Count());

        foreach (var expectedLink in expectedLinks)
        {
            var actualLink = Assert.Single(actualLinks, 
                x => x.Attribute("Source")?.Value == expectedLink.Source 
                    && x.Attribute("Target")?.Value == expectedLink.Target);

            Assert.EndsWith("Link", actualLink.Name.ToString());

            Assert.Equal(expectedLink.Label, actualLink.Attribute("Label")?.Value);
            Assert.Equal(expectedLink.Category, actualLink.Attribute("Category")?.Value);
        }
    }

    private static void AssertCategories(IEnumerable<Category> expectedCategories, IEnumerable<XElement> actualCategories)
    {
        Assert.Equal(expectedCategories.Count(), actualCategories.Count());
        
        foreach (var expectedCategory in expectedCategories)
        {
            var actualCategory = Assert.Single(actualCategories, x => x.Attribute("Id")?.Value == expectedCategory.Id);

            Assert.EndsWith("Category", actualCategory.Name.ToString());

            // TODO: Create test color provider so this can be asserted (e.g. TestColorProvider).
            //Assert.Equal(expectedCategory.Background, actualCategory.Attribute("Background")?.Value);
            Assert.Equal(expectedCategory.Stroke, actualCategory.Attribute("Stroke")?.Value);
            Assert.Equal(expectedCategory.Label, actualCategory.Attribute("Label")?.Value);
        }
    }

    private class Node
    {
        public string? Id { get; set; }
        public string? Label { get; set; }
        public string? Category { get; set; }
        public string? Reference { get; set; }
    }

    private class Link
    {
        public string? Source { get; set; }
        public string? Target { get; set; }
        public string? Label { get; set; }
        public string? Category { get; set; }
    }

    private class Category
    {
        public string? Id { get; set; }
        public string? Background { get; set; }
        public string? Stroke { get; set; }
        public string? Label { get; set; }
    }
}

