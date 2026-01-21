using DynamoMapper.Runtime;

namespace DynamoMapper.Generator.PropertyMapping.Models;

/// <summary>Defines the strategy for mapping a C# type to AttributeValue extension methods.</summary>
/// <param name="TypeName">
///     The type name used in method names (e.g., "String", "Int", "DateTime",
///     "Enum", "NestedObject", "NestedList", "NestedMap").
/// </param>
/// <param name="GenericArgument">
///     Generic type argument for the method call (e.g., "&lt;OrderStatus&gt;
///     " for enums).
/// </param>
/// <param name="NullableModifier">
///     Modifier added to method names for nullable types (e.g., "Nullable"
///     or empty string).
/// </param>
/// <param name="FromTypeSpecificArgs">
///     Type-specific arguments for deserialization (FromItem), like
///     format strings or default values.
/// </param>
/// <param name="ToTypeSpecificArgs">
///     Type-specific arguments for serialization (ToItem), like format
///     strings.
/// </param>
/// <param name="KindOverride">
///     Optional DynamoKind override from DynamoFieldOptions. When present,
///     this value is passed as an additional argument to Get/Set methods.
/// </param>
/// <param name="NestedMapping">
///     For nested object types, contains information about how to map the nested object
///     (either via an external mapper or inline code generation).
/// </param>
/// <param name="CollectionInfo">
///     For collection types with nested object elements, contains collection metadata
///     including the element nested mapping information.
/// </param>
internal sealed record TypeMappingStrategy(
    string TypeName,
    string GenericArgument,
    string NullableModifier,
    string[] FromTypeSpecificArgs,
    string[] ToTypeSpecificArgs,
    DynamoKind? KindOverride = null,
    NestedMappingInfo? NestedMapping = null,
    CollectionInfo? CollectionInfo = null
);
