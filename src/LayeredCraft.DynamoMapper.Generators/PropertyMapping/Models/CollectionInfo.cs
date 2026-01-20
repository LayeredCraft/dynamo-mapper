using DynamoMapper.Runtime;
using Microsoft.CodeAnalysis;

namespace DynamoMapper.Generator.PropertyMapping.Models;

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
/// <param name="IsArray">
///     True if the original property type is an array (T[]), false otherwise.
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
    bool IsArray = false,
    NestedMappingInfo? ElementNestedMapping = null
);

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