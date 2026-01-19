using DynamoMapper.Generator.Diagnostics;
using DynamoMapper.Generator.PropertyMapping.Models;
using Microsoft.CodeAnalysis;

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

        // Use shared MemberAnalyzer for common analysis
        var memberInfoResult = MemberAnalyzer.AnalyzeProperty(propertySymbol, context);
        if (!memberInfoResult.IsSuccess)
            return DiagnosticResult<PropertyAnalysis>.Failure(memberInfoResult.Error!);

        var memberInfo = memberInfoResult.Value!;

        // Detect property-specific accessors
        var hasGetter = propertySymbol.GetMethod is not null;
        var hasSetter = propertySymbol.SetMethod is not null; // includes init-only setters

        var isRequired = propertySymbol.IsRequired;
        var isInitOnly = propertySymbol.SetMethod?.IsInitOnly ?? false;

        return new PropertyAnalysis(
            memberInfo.MemberName,
            memberInfo.MemberType,
            memberInfo.UnderlyingType,
            memberInfo.Nullability,
            memberInfo.FieldOptions,
            hasGetter,
            hasSetter,
            isRequired,
            isInitOnly,
            memberInfo.HasDefaultValue,
            memberInfo
        );
    }
}
