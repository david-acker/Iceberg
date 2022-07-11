using System.Xml.Linq;

namespace Iceberg.Export;

public class DGMLBuilder
{
    private readonly XNamespace _xmlns = @"http://schemas.microsoft.com/vs/2009/dgml";

    public string Title { get; set; }

    private readonly ICollection<XElement> _nodes = new List<XElement>();
    private readonly ICollection<XElement> _links = new List<XElement>();
    private readonly ICollection<XElement> _categories = new List<XElement>();

    public DGMLBuilder(string title = "Dependency Graph")
    {
        Title = title;
    }

    public void AddNode(
        string elementId,
        string category,
        string? label = null,
        string? referencePath = null)
    {
        var node = new XElement(_xmlns + "Node",
            new XAttribute("Id", elementId),
            new XAttribute("Label", label ?? elementId),
            new XAttribute("Category", category));

        if (referencePath is not null)
            node.SetAttributeValue("Reference", referencePath);

        _nodes.Add(node);
    }

    public void AddLink(
        string source,
        string target,
        string? label = null,
        string? category = null)
    {
        var link = new XElement(_xmlns + "Link",
            new XAttribute("Source", source),
            new XAttribute("Target", target));

        if (label is not null)
            link.SetAttributeValue("Label", label);

        if (category is not null)
            link.SetAttributeValue("Category", category);

        _links.Add(link);
    }

    public void AddCategory(
        string categoryId,
        string backgroundColor,
        string? strokeColor = null,
        string? label = null)
    {
        var category = new XElement(_xmlns + "Category",
            new XAttribute("Id", categoryId),
            new XAttribute("Background", backgroundColor));

        if (strokeColor is not null)
            category.SetAttributeValue("Stroke", strokeColor);

        if (label is not null)
            category.SetAttributeValue("Label", label);

        _categories.Add(category);
    }

    public XElement Content =>
        new(_xmlns + "DirectedGraph",
            new XElement(_xmlns + "Nodes", _nodes),
            new XElement(_xmlns + "Links", _links),
            new XElement(_xmlns + "Categories", _categories),
            new XAttribute("Title", Title));
}
