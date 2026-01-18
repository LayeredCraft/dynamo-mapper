using DynamoMapper.Generator.PropertyMapping.Models;
using Microsoft.CodeAnalysis;

namespace DynamoMapper.Generator.ConstructorMapping.Models;

/// <summary>
///     Represents the semantic analysis of a constructor parameter, including its matched property.
/// </summary>
/// <param name="MemberInfo">Shared member analysis (type, nullability, etc.).</param>
/// <param name="Ordinal">The zero-based position of this parameter in the constructor.</param>
/// <param name="MatchedProperty">The property this parameter corresponds to (matched via case-insensitive name comparison).</param>
internal sealed record ParameterAnalysis(
    MemberAnalysis MemberInfo,
    int Ordinal,
    IPropertySymbol? MatchedProperty
);
