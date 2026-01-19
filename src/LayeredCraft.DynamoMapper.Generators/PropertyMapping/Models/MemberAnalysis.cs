using Microsoft.CodeAnalysis;

namespace DynamoMapper.Generator.PropertyMapping.Models;

/// <summary>
///     Represents semantic information that is common to both property symbols and parameter symbols.
///     This shared analysis enables code reuse in type mapping logic.
/// </summary>
/// <param name="MemberName">The name of the property or parameter.</param>
/// <param name="MemberType">The full type of the member (may be nullable).</param>
/// <param name="UnderlyingType">The underlying type, unwrapped from Nullable&lt;T&gt; if applicable.</param>
/// <param name="Nullability">Nullability information for the member type.</param>
/// <param name="FieldOptions">Field-level options from DynamoFieldAttribute (properties only, null for parameters).</param>
/// <param name="HasDefaultValue">True if the member has a default value.</param>
internal sealed record MemberAnalysis(
    string MemberName,
    ITypeSymbol MemberType,
    ITypeSymbol UnderlyingType,
    PropertyNullabilityInfo Nullability,
    DynamoFieldOptions? FieldOptions,
    bool HasDefaultValue
);
