using DynamoMapper.Generator.Diagnostics;
using DynamoMapper.Generator.PropertyMapping.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DynamoMapper.Generator.PropertyMapping;

/// <summary>
///     Performs pure Roslyn symbol analysis on property symbols. Extracts property information
///     without any mapping logic.
/// </summary>
internal static class PropertyAnalyzer
{
    /// <summary>Analyzes a property symbol to extract semantic information.</summary>
    /// <param name="propertySymbol">The property symbol to analyze.</param>
    /// <param name="context">The generator context.</param>
    /// <returns>A diagnostic result containing the property analysis.</returns>
    internal static DiagnosticResult<PropertyAnalysis> Analyze(
        IPropertySymbol propertySymbol,
        GeneratorContext context
    )
    {
        context.ThrowIfCancellationRequested();

        var nullability = AnalyzeNullability(propertySymbol);
        var underlyingType = UnwrapNullableType(propertySymbol.Type);

        // Lookup field-level overrides from DynamoFieldAttribute
        context.FieldOptions.TryGetValue(propertySymbol.Name, out var fieldOptions);

        // Detect property accessors
        var hasGetter = propertySymbol.GetMethod is not null;
        var hasSetter = propertySymbol.SetMethod is not null; // includes init-only setters

        var isRequired = propertySymbol.IsRequired;
        var isInitOnly = propertySymbol.SetMethod?.IsInitOnly ?? false;

        var hasDefaultValue = HasDefaultValue(propertySymbol);

        return new PropertyAnalysis(
            propertySymbol.Name,
            propertySymbol.Type,
            underlyingType,
            nullability,
            fieldOptions,
            hasGetter,
            hasSetter,
            isRequired,
            isInitOnly,
            hasDefaultValue
        );
    }

    /// <summary>Analyzes the nullability characteristics of a property.</summary>
    private static PropertyNullabilityInfo AnalyzeNullability(IPropertySymbol propertySymbol)
    {
        var type = propertySymbol.Type;
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

    private static bool HasDefaultValue(IPropertySymbol property) =>
        property.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax()
            is PropertyDeclarationSyntax { Initializer: not null };

    /// <summary>
    ///     Unwraps Nullable&lt;T&gt; to get the underlying type T. If the type is not nullable,
    ///     returns the type unchanged.
    /// </summary>
    private static ITypeSymbol UnwrapNullableType(ITypeSymbol type)
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
