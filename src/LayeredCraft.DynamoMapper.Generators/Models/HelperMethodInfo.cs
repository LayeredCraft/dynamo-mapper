using DynamoMapper.Generator.PropertyMapping.Models;

namespace DynamoMapper.Generator.Models;

/// <summary>Represents a helper method that maps a nested type to/from AttributeValue dictionary.</summary>
internal sealed record HelperMethodInfo(
    string MethodName, // e.g., "ToItem_Address"
    string ModelFullyQualifiedType, // e.g., "global::MyNamespace.Address"
    NestedInlineInfo InlineInfo, // Property specs for the nested type
    HelperMethodDirection Direction // ToItem or FromItem
);

internal enum HelperMethodDirection
{
    ToItem,
    FromItem,
}
