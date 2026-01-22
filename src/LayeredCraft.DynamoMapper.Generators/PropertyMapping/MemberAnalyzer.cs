using DynamoMapper.Generator.Diagnostics;
using DynamoMapper.Generator.PropertyMapping.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DynamoMapper.Generator.PropertyMapping;

/// <summary>
///     Performs shared semantic analysis on property and parameter symbols. Extracts common information
///     needed for type mapping without any mapping logic.
/// </summary>
internal static class MemberAnalyzer
{
    /// <summary>Analyzes a property symbol to extract shared member information.</summary>
    /// <param name="propertySymbol">The property symbol to analyze.</param>
    /// <param name="context">The generator context.</param>
    /// <returns>A diagnostic result containing the member analysis.</returns>
    internal static DiagnosticResult<MemberAnalysis> AnalyzeProperty(
        IPropertySymbol propertySymbol,
        GeneratorContext context
    )
    {
        context.ThrowIfCancellationRequested();

        var nullability = AnalyzeNullability(propertySymbol.Type);
        var underlyingType = UnwrapNullableType(propertySymbol.Type);

        // Lookup field-level overrides from DynamoFieldAttribute
        context.FieldOptions.TryGetValue(propertySymbol.Name, out var fieldOptions);

        var hasDefaultValue = HasPropertyDefaultValue(propertySymbol);

        return new MemberAnalysis(
            propertySymbol.Name,
            propertySymbol.Type,
            underlyingType,
            nullability,
            fieldOptions,
            hasDefaultValue
        );
    }

    /// <summary>Analyzes a parameter symbol to extract shared member information.</summary>
    /// <param name="parameterSymbol">The parameter symbol to analyze.</param>
    /// <param name="context">The generator context.</param>
    /// <returns>A diagnostic result containing the member analysis.</returns>
    internal static DiagnosticResult<MemberAnalysis> AnalyzeParameter(
        IParameterSymbol parameterSymbol,
        GeneratorContext context
    )
    {
        context.ThrowIfCancellationRequested();

        var nullability = AnalyzeNullability(parameterSymbol.Type);
        var underlyingType = UnwrapNullableType(parameterSymbol.Type);

        // Parameters don't have field-level options (only properties do)
        var hasDefaultValue = parameterSymbol.HasExplicitDefaultValue;

        return new MemberAnalysis(
            parameterSymbol.Name,
            parameterSymbol.Type,
            underlyingType,
            nullability,
            null, // Parameters don't have DynamoFieldAttribute
            hasDefaultValue
        );
    }

    /// <summary>Analyzes the nullability characteristics of a type.</summary>
    internal static PropertyNullabilityInfo AnalyzeNullability(ITypeSymbol type)
    {
        var isReferenceType = type.IsReferenceType;
        var annotation = type.NullableAnnotation;

        // A type is nullable if:
        // 1. It's Nullable<T> (value type)
        // 2. It's a reference type with Annotated nullable annotation
        var isNullableType =
            type
                is INamedTypeSymbol
            {
                OriginalDefinition.SpecialType: SpecialType.System_Nullable_T,
            }
            || (isReferenceType && annotation == NullableAnnotation.Annotated);

        return new PropertyNullabilityInfo(isNullableType, isReferenceType, annotation);
    }

    /// <summary>Checks if a property has a default value (initializer).</summary>
    private static bool HasPropertyDefaultValue(IPropertySymbol property) =>
        property.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax()
            is PropertyDeclarationSyntax { Initializer: not null };

    /// <summary>
    ///     Unwraps Nullable&lt;T&gt; to get the underlying type T. If the type is not nullable,
    ///     returns the type unchanged.
    /// </summary>
    internal static ITypeSymbol UnwrapNullableType(ITypeSymbol type)
    {
        if (
            type is INamedTypeSymbol
            {
                OriginalDefinition.SpecialType: SpecialType.System_Nullable_T,
            } namedType
        )
            return namedType.TypeArguments[0];

        return type;
    }
}
