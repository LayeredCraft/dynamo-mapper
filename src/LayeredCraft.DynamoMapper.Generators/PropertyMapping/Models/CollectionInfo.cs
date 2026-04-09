using LayeredCraft.DynamoMapper.Runtime;
using Microsoft.CodeAnalysis;

namespace LayeredCraft.DynamoMapper.Generator.PropertyMapping.Models;

/// <summary>
/// Represents metadata about a collection type property.
/// </summary>
/// <param name="Category">The category of collection (List, Map, or Set).</param>
/// <param name="ElementType">
///     The element type for lists and sets, or the value type for maps.
/// </param>
/// <param name="TargetKind">
///     The DynamoDB AttributeValue kind this collection maps to (L, M, SS, NS, or BS).
/// </param>
/// <param name="KeyType">
///     For map types only, the key type (must be string).
/// </param>
/// <param name="ReadMaterialization">
///     Describes any additional CLR-shape materialization required after deserializing the
///     DynamoDB representation.
/// </param>
/// <param name="ElementNestedMapping">
///     For collections of nested objects, contains the nested mapping info for the element type.
///     Null for primitive element types.
/// </param>
internal sealed record CollectionInfo(
    CollectionCategory Category,
    ITypeSymbol ElementType,
    DynamoKind TargetKind,
    ITypeSymbol? KeyType = null,
    CollectionReadMaterialization ReadMaterialization = CollectionReadMaterialization.None,
    NestedMappingInfo? ElementNestedMapping = null
);

/// <summary>
///     Describes how a deserialized collection result should be materialized back into the
///     declared CLR shape.
/// </summary>
internal enum CollectionReadMaterialization
{
    None,
    Array,
    HashSet,
}

/// <summary>
/// Categorizes collection types by their DynamoDB mapping behavior.
/// </summary>
internal enum CollectionCategory
{
    /// <summary>
    /// List-like collections (arrays, List&lt;T&gt;, IEnumerable&lt;T&gt;, etc.) → DynamoKind.L
    /// </summary>
    List,

    /// <summary>
    /// Map-like collections (Dictionary&lt;string, T&gt;, IDictionary&lt;string, T&gt;) → DynamoKind.M
    /// </summary>
    Map,

    /// <summary>
    /// Set-like collections (HashSet&lt;T&gt;, ISet&lt;T&gt;) → DynamoKind.SS/NS/BS based on element type
    /// </summary>
    Set
}
