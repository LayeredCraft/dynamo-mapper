using Microsoft.CodeAnalysis;

namespace DynamoMapper.Generator.PropertyMapping.Models;

/// <summary>
///     Captures nullability information about a property type. Renamed to avoid conflict with
///     Microsoft.CodeAnalysis.NullabilityInfo.
/// </summary>
/// <param name="IsNullableType">True if the type is Nullable&lt;T&gt; or an annotated reference type.</param>
/// <param name="IsReferenceType">True if the type is a reference type.</param>
/// <param name="Annotation">The nullable annotation from Roslyn.</param>
internal sealed record PropertyNullabilityInfo(
    bool IsNullableType,
    bool IsReferenceType,
    NullableAnnotation Annotation
);
