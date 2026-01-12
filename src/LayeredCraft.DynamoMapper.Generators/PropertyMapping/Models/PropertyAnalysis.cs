using Microsoft.CodeAnalysis;

namespace DynamoMapper.Generator.PropertyMapping.Models;

/// <summary>
///     Represents the semantic analysis of a property symbol. This is pure Roslyn symbol
///     information with no mapping logic.
/// </summary>
/// <param name="PropertyName">The name of the property.</param>
/// <param name="PropertyType">The property's type symbol.</param>
/// <param name="UnderlyingType">The underlying type, unwrapped from Nullable&lt;T&gt; if applicable.</param>
/// <param name="Nullability">Nullability information about the property.</param>
/// <param name="FieldOptions">Optional field-level overrides from DynamoFieldAttribute.</param>
internal sealed record PropertyAnalysis(
    string PropertyName,
    ITypeSymbol PropertyType,
    ITypeSymbol UnderlyingType,
    PropertyNullabilityInfo Nullability,
    DynamoFieldOptions? FieldOptions
);
