using Microsoft.CodeAnalysis;

namespace DynamoMapper.Generator.PropertyMapping.Models;

/// <summary>
///     Represents the semantic analysis of a property symbol. This is pure Roslyn symbol
///     information with no mapping logic.
/// </summary>
internal sealed record PropertyAnalysis(
    string PropertyName,
    ITypeSymbol PropertyType,
    ITypeSymbol UnderlyingType,
    PropertyNullabilityInfo Nullability,
    DynamoFieldOptions? FieldOptions,
    bool HasGetter,
    bool HasSetter,
    bool IsRequired,
    bool IsInitOnly,
    bool HasDefaultValue,
    // Shared member analysis for code reuse with constructor parameters
    MemberAnalysis MemberInfo
);
