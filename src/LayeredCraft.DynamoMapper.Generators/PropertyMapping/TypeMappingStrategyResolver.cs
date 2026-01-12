using DynamoMapper.Generator.Diagnostics;
using DynamoMapper.Generator.Models;
using DynamoMapper.Generator.PropertyMapping.Models;
using DynamoMapper.Runtime;
using Microsoft.CodeAnalysis;
using WellKnownType = DynamoMapper.Generator.WellKnownTypes.WellKnownTypeData.WellKnownType;

namespace DynamoMapper.Generator.PropertyMapping;

/// <summary>
///     Resolves C# types to AttributeValue extension method mapping strategies. Contains the core
///     type-to-method mapping logic.
/// </summary>
internal static class TypeMappingStrategyResolver
{
    /// <summary>Resolves a type mapping strategy for the given property analysis.</summary>
    /// <param name="analysis">The property analysis containing type information.</param>
    /// <param name="context">The generator context.</param>
    /// <returns>
    ///     A diagnostic result containing the type mapping strategy, or a failure for unsupported
    ///     types.
    /// </returns>
    internal static DiagnosticResult<TypeMappingStrategy> Resolve(
        PropertyAnalysis analysis,
        GeneratorContext context
    )
    {
        context.ThrowIfCancellationRequested();

        // Check for Kind override first - it takes precedence over type inference
        if (analysis.FieldOptions?.Kind is DynamoKind overrideKind)
            return CreateStrategyForKindOverride(overrideKind, analysis, context);

        return analysis.UnderlyingType switch
        {
            { SpecialType: SpecialType.System_String } => CreateStrategy(
                "String",
                analysis.Nullability
            ),
            { SpecialType: SpecialType.System_Boolean } => CreateStrategy(
                "Bool",
                analysis.Nullability
            ),
            { SpecialType: SpecialType.System_Int32 } => CreateStrategy(
                "Int",
                analysis.Nullability
            ),
            { SpecialType: SpecialType.System_Int64 } => CreateStrategy(
                "Long",
                analysis.Nullability
            ),
            { SpecialType: SpecialType.System_Single } => CreateStrategy(
                "Float",
                analysis.Nullability
            ),
            { SpecialType: SpecialType.System_Double } => CreateStrategy(
                "Double",
                analysis.Nullability
            ),
            { SpecialType: SpecialType.System_Decimal } => CreateStrategy(
                "Decimal",
                analysis.Nullability
            ),
            { SpecialType: SpecialType.System_DateTime } =>
                $"\"{context.MapperOptions.DateTimeFormat}\"".Map(dateFmt =>
                    CreateStrategy(
                        "DateTime",
                        analysis.Nullability,
                        fromArg: dateFmt,
                        toArg: dateFmt
                    )
                ),
            INamedTypeSymbol t
                when t.IsAssignableTo(WellKnownType.System_DateTimeOffset, context) =>
                $"\"{context.MapperOptions.DateTimeFormat}\"".Map(dateFmt =>
                    CreateStrategy(
                        "DateTimeOffset",
                        analysis.Nullability,
                        fromArg: dateFmt,
                        toArg: dateFmt
                    )
                ),
            INamedTypeSymbol t when t.IsAssignableTo(WellKnownType.System_Guid, context) =>
                CreateStrategy("Guid", analysis.Nullability),
            INamedTypeSymbol t when t.IsAssignableTo(WellKnownType.System_TimeSpan, context) =>
                CreateStrategy("TimeSpan", analysis.Nullability),
            INamedTypeSymbol { TypeKind: TypeKind.Enum } enumType => CreateEnumStrategy(
                enumType,
                analysis,
                context
            ),
            _ => DiagnosticResult<TypeMappingStrategy>.Failure(
                DiagnosticDescriptors.CannotConvertFromAttributeValue,
                analysis.PropertyType.Locations.FirstOrDefault()?.CreateLocationInfo(),
                analysis.PropertyName,
                analysis.PropertyType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
            ),
        };
    }

    /// <summary>Creates a type mapping strategy with optional type-specific arguments.</summary>
    /// <param name="typeName">The type name for the extension method (e.g., "String", "Int").</param>
    /// <param name="nullability">The nullability information.</param>
    /// <param name="genericArg">The generic type argument (empty for non-generic methods).</param>
    /// <param name="fromArg">Optional argument for FromItem method (e.g., format string).</param>
    /// <param name="toArg">Optional argument for ToItem method (e.g., format string).</param>
    private static TypeMappingStrategy CreateStrategy(
        string typeName,
        PropertyNullabilityInfo nullability,
        string genericArg = "",
        string? fromArg = null,
        string? toArg = null
    )
    {
        var nullableModifier = nullability.IsNullableType ? "Nullable" : string.Empty;

        string[] fromArgs = fromArg != null ? [fromArg] : [];
        string[] toArgs = toArg != null ? [toArg] : [];

        return new TypeMappingStrategy(typeName, genericArg, nullableModifier, fromArgs, toArgs);
    }

    /// <summary>
    ///     Creates a type mapping strategy for enum types. Enums require special handling for default
    ///     values and format strings.
    /// </summary>
    private static DiagnosticResult<TypeMappingStrategy> CreateEnumStrategy(
        INamedTypeSymbol enumType,
        PropertyAnalysis analysis,
        GeneratorContext context
    )
    {
        var enumName = enumType.QualifiedName;
        var enumFormat = $"\"{context.MapperOptions.EnumFormat}\"";
        var nullableModifier = analysis.Nullability.IsNullableType ? "Nullable" : "";

        // Non-nullable enums need a default value in FromItem
        // Nullable enums don't need a default value (they can be null)
        string[] fromArgs = analysis.Nullability.IsNullableType
            ? [enumFormat]
            : [$"{enumName}.{enumType.MemberNames.First()}", enumFormat];

        string[] toArgs = [enumFormat];

        return new TypeMappingStrategy("Enum", $"<{enumName}>", nullableModifier, fromArgs, toArgs);
    }

    /// <summary>
    ///     Creates a type mapping strategy for a Kind override. The Kind only affects serialization
    ///     format, not the C# type mapping - method names are still based on the property's C# type.
    /// </summary>
    private static DiagnosticResult<TypeMappingStrategy> CreateStrategyForKindOverride(
        DynamoKind kind,
        PropertyAnalysis analysis,
        GeneratorContext context
    )
    {
        // Validate that the Kind is supported
        if (kind is DynamoKind.L or DynamoKind.M or DynamoKind.SS or DynamoKind.NS or DynamoKind.BS)
            return DiagnosticResult<TypeMappingStrategy>.Failure(
                DiagnosticDescriptors.CannotConvertFromAttributeValue,
                analysis.PropertyType.Locations.FirstOrDefault()?.CreateLocationInfo(),
                analysis.PropertyName,
                $"DynamoKind.{kind} (not supported in Phase 1)"
            );

        // Resolve the strategy based on the actual C# type (not the Kind)
        // Then add the Kind as an override parameter
        var baseStrategyResult = analysis.UnderlyingType switch
        {
            { SpecialType: SpecialType.System_String } => CreateStrategy(
                "String",
                analysis.Nullability
            ),
            { SpecialType: SpecialType.System_Boolean } => CreateStrategy(
                "Bool",
                analysis.Nullability
            ),
            { SpecialType: SpecialType.System_Int32 } => CreateStrategy(
                "Int",
                analysis.Nullability
            ),
            { SpecialType: SpecialType.System_Int64 } => CreateStrategy(
                "Long",
                analysis.Nullability
            ),
            { SpecialType: SpecialType.System_Single } => CreateStrategy(
                "Float",
                analysis.Nullability
            ),
            { SpecialType: SpecialType.System_Double } => CreateStrategy(
                "Double",
                analysis.Nullability
            ),
            { SpecialType: SpecialType.System_Decimal } => CreateStrategy(
                "Decimal",
                analysis.Nullability
            ),
            _ => null,
        };

        if (baseStrategyResult is null)
            return DiagnosticResult<TypeMappingStrategy>.Failure(
                DiagnosticDescriptors.CannotConvertFromAttributeValue,
                analysis.PropertyType.Locations.FirstOrDefault()?.CreateLocationInfo(),
                analysis.PropertyName,
                analysis.PropertyType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
            );

        // Add the Kind override to the base strategy
        return new TypeMappingStrategy(
            baseStrategyResult.TypeName,
            baseStrategyResult.GenericArgument,
            baseStrategyResult.NullableModifier,
            baseStrategyResult.FromTypeSpecificArgs,
            baseStrategyResult.ToTypeSpecificArgs,
            kind
        );
    }
}
