using System.Diagnostics.CodeAnalysis;

namespace Iceberg.Map.Metadata;

public class MethodMetadataComparer : IEqualityComparer<MethodMetadata>
{
    public bool Equals(MethodMetadata? x, MethodMetadata? y)
    {
        return x?.DisplayName == y?.DisplayName;
    }

    public int GetHashCode([DisallowNull] MethodMetadata obj)
    {
        return obj.DisplayName.GetHashCode();
    }
}