namespace Iceberg.Map.Metadata;

public class MethodMetadata
{
    public MethodMetadata(string displayName, string sourcePath)
    {
        DisplayName = displayName;
        SourcePath = sourcePath;

        _methodName = new Lazy<string>(() => GetMethodName(DisplayName));
        _className = new Lazy<string>(() => GetClassName(DisplayName));
        _namespaceComponents = new Lazy<string[]>(() => GetNamespaceComponents(DisplayName));
        _typeParameters = new Lazy<string[]>(() => GetTypeParameters(DisplayName));
        _parameters = new Lazy<string[]>(() => GetParameters(DisplayName));
    }

    private readonly Lazy<string> _methodName;
    public string MethodName => _methodName.Value;

    private static string GetMethodName(string entryPointDisplayName)
    {
        var searchString = entryPointDisplayName
           .Split('(')[0]
           .Split('<')[0];

        return searchString.Split('.')[^1];
    }

    private readonly Lazy<string> _className;
    public string ClassName => _className.Value;

    private static string GetClassName(string entryPointDisplayName)
    {
        var searchString = entryPointDisplayName
           .Split('(')[0]
           .Split('<')[0];

        return searchString.Split('.')[^2];
    }

    private readonly Lazy<string[]> _namespaceComponents;
    public string[] NamespaceComponents => _namespaceComponents.Value;

    private static string[] GetNamespaceComponents(string entryPointDisplayName)
    {
        var searchString = entryPointDisplayName
            .Split('(')[0]
            .Split('<')[0];

        return searchString.Split('.')[..^2];
    }

    public string SourcePath { get; init; }

    private readonly Lazy<string[]> _typeParameters;
    public string[] TypeParameters => _typeParameters.Value;

    private static string[] GetTypeParameters(string entryPointDisplayName)
    {
        var searchString = entryPointDisplayName.Split('(')[0];

        var startIndex = searchString.IndexOf('<');
        if (startIndex == -1)
            return Array.Empty<string>();

        var endIndex = searchString.LastIndexOf('>');

        return searchString[(startIndex + 1)..endIndex]
            .Split(',', StringSplitOptions.TrimEntries)
            .ToArray();
    }

    private readonly Lazy<string[]> _parameters;
    public string[] Parameters => _parameters.Value;

    private static string[] GetParameters(string entryPointDisplayName)
    {
        var searchString = entryPointDisplayName
            .Split('(')[1]
            .TrimEnd(')');

        if (string.IsNullOrWhiteSpace(searchString))
            return Array.Empty<string>();

        return searchString.Split(',', StringSplitOptions.TrimEntries)
            .ToArray();
    }

    public string ReturnType { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;
}
