namespace Iceberg.Map.DependencyMapper;

public static class MethodEntryPointNameStringExtensions
{
    public static string GetMethodName(this string entryPointName)
    {
        var searchString = entryPointName
           .Split('(')[0]
           .Split('<')[0];

        return searchString.Split('.')[^1];
    }

    public static string GetClassName(this string entryPointName)
    {
        var searchString = entryPointName
           .Split('(')[0]
           .Split('<')[0];

        return searchString.Split('.')[^2];
    }

    public static string[] GetNamespace(this string entryPointName)
    {
        var searchString = entryPointName
            .Split('(')[0]
            .Split('<')[0];

        return searchString.Split('.')[..^2];
    }

    public static string[] GetTypeParameters(this string entryPointName)
    {
        var searchString = entryPointName.Split('(')[0].TrimEnd(')');

        if (!string.IsNullOrWhiteSpace(searchString))
            return Array.Empty<string>();

        return searchString.Split(',', StringSplitOptions.TrimEntries)
            .ToArray();
    }

    public static string[] GetParameters(this string entryPointName)
    {
        var searchString = entryPointName.Split('(')[1];

        var startIndex = searchString.IndexOf('<');
        if (startIndex == -1)
            return Array.Empty<string>();

        var endIndex = searchString.LastIndexOf('>');

        return searchString[startIndex..endIndex]
            .Split(',', StringSplitOptions.TrimEntries)
            .ToArray();
    }
}
