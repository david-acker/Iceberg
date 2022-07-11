namespace Iceberg.Map.Metadata;

public class MethodDependencyMap : Dictionary<MethodMetadata, HashSet<MethodMetadata>>
{
    public MethodDependencyMap(IEqualityComparer<MethodMetadata> comparer) : base(comparer)
    {
    }

    public MethodDependencyMap() : base(new MethodMetadataComparer())
    {
    }

    // TODO: Encapsulate any collection specific logic here.
}