using DynamoMapper.Generator.Models;
using LayeredCraft.SourceGeneratorTools.Types;

namespace DynamoMapper.Generator.PropertyMapping.Models;

/// <summary>
///     Base type for nested object mapping information. Used to determine how a nested object
///     should be mapped (via external mapper or inline code generation).
/// </summary>
internal abstract record NestedMappingInfo;

/// <summary>
///     Indicates that a nested object should be mapped using an existing mapper class.
/// </summary>
/// <param name="Mapper">Reference to the mapper class to use.</param>
internal sealed record MapperBasedNesting(MapperReference Mapper) : NestedMappingInfo;

/// <summary>
///     Indicates that a nested object should be mapped using inline generated code.
/// </summary>
/// <param name="Info">Information about the nested type and its properties.</param>
internal sealed record InlineNesting(NestedInlineInfo Info) : NestedMappingInfo;

/// <summary>
///     Information needed for inline nested object code generation.
/// </summary>
/// <param name="ModelFullyQualifiedType">
///     The fully qualified type name of the nested model (e.g., "global::MyNamespace.Address").
/// </param>
/// <param name="Properties">
///     The properties of the nested type with their mapping specifications.
/// </param>
internal sealed record NestedInlineInfo(
    string ModelFullyQualifiedType,
    EquatableArray<NestedPropertySpec> Properties
);

/// <summary>
///     Specification for a single property within a nested object during inline mapping.
/// </summary>
/// <param name="PropertyName">The C# property name.</param>
/// <param name="DynamoKey">The DynamoDB attribute name.</param>
/// <param name="Strategy">The type mapping strategy for this property (can itself be nested).</param>
/// <param name="NestedMapping">If the property is itself a nested object, its mapping info.</param>
internal sealed record NestedPropertySpec(
    string PropertyName,
    string DynamoKey,
    TypeMappingStrategy? Strategy,
    NestedMappingInfo? NestedMapping = null
);
