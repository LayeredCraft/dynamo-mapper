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
    internal static DiagnosticResult<TypeMappingStrategy?> Resolve(
        PropertyAnalysis analysis,
        GeneratorContext context
    )
    {
        context.ThrowIfCancellationRequested();

        // Skip type resolution if both custom methods are provided
        if (analysis.FieldOptions is { ToMethod: not null, FromMethod: not null })
            return DiagnosticResult<TypeMappingStrategy?>.Success(null);

        // Skip validation if property won't be used in any generated methods
        // Also skip if a custom method is provided for that direction (no auto-mapping needed)
        var willBeUsedInToItem =
            context.HasToItemMethod
            && analysis.HasGetter
            && analysis.FieldOptions?.ToMethod == null;

        var willBeUsedInFromItem =
            context.HasFromItemMethod
            && analysis.HasSetter
            && analysis.FieldOptions?.FromMethod == null;

        if (!willBeUsedInToItem && !willBeUsedInFromItem)
            return DiagnosticResult<TypeMappingStrategy?>.Success(null);

        // Check if property should be ignored based on DynamoIgnoreOptions
        if (context.IgnoreOptions.TryGetValue(analysis.PropertyName, out var ignoreOptions))
        {
            var shouldIgnoreToItem =
                ignoreOptions.Ignore is IgnoreMapping.All or IgnoreMapping.FromModel;
            var shouldIgnoreFromItem =
                ignoreOptions.Ignore is IgnoreMapping.All or IgnoreMapping.ToModel;

            // If property should be ignored in all relevant directions, skip mapping
            if (
                (!willBeUsedInToItem || shouldIgnoreToItem)
                && (!willBeUsedInFromItem || shouldIgnoreFromItem)
            )
                return DiagnosticResult<TypeMappingStrategy?>.Success(null);
        }

        // Try collection analysis first before scalar type resolution
        var collectionInfo = CollectionTypeAnalyzer.Analyze(analysis.UnderlyingType, context);
        if (collectionInfo is not null)
        {
            return CreateCollectionStrategy(collectionInfo, analysis, context);
        }

        // Resolve the base type mapping strategy (existing logic unchanged)
        var strategyResult = analysis.UnderlyingType switch
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
                $"\"{context.MapperOptions.GuidFormat}\"".Map(guidFmt =>
                    CreateStrategy("Guid", analysis.Nullability, fromArg: guidFmt, toArg: guidFmt)
                ),
            INamedTypeSymbol t when t.IsAssignableTo(WellKnownType.System_TimeSpan, context) =>
                $"\"{context.MapperOptions.TimeSpanFormat}\"".Map(timeFmt =>
                    CreateStrategy(
                        "TimeSpan",
                        analysis.Nullability,
                        fromArg: timeFmt,
                        toArg: timeFmt
                    )
                ),
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

        // If type resolution succeeded and Kind override exists, augment the strategy
        return strategyResult.Bind<TypeMappingStrategy?>(strategy =>
            analysis.FieldOptions?.Kind is { } kind
                ? strategy with
                {
                    KindOverride = kind,
                }
                : strategy
        );
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
    ///     Creates a type mapping strategy for collection types (lists, maps, sets).
    /// </summary>
    private static DiagnosticResult<TypeMappingStrategy?> CreateCollectionStrategy(
        CollectionInfo collectionInfo,
        PropertyAnalysis analysis,
        GeneratorContext context
    )
    {
        // Validate element type is supported primitive
        if (!CollectionTypeAnalyzer.IsValidElementType(collectionInfo.ElementType, context))
        {
            return DiagnosticResult<TypeMappingStrategy?>.Failure(
                DiagnosticDescriptors.UnsupportedCollectionElementType,
                analysis.PropertyType.Locations.FirstOrDefault()?.CreateLocationInfo(),
                analysis.PropertyName,
                collectionInfo.ElementType.ToDisplayString()
            );
        }

        // For maps, validate key type is string
        if (
            collectionInfo.Category == CollectionCategory.Map
            && collectionInfo.KeyType?.SpecialType != SpecialType.System_String
        )
        {
            return DiagnosticResult<TypeMappingStrategy?>.Failure(
                DiagnosticDescriptors.DictionaryKeyMustBeString,
                analysis.PropertyType.Locations.FirstOrDefault()?.CreateLocationInfo(),
                analysis.PropertyName,
                collectionInfo.KeyType?.ToDisplayString() ?? "unknown"
            );
        }

        // Validate Kind override compatibility if present
        if (analysis.FieldOptions?.Kind is { } kindOverride)
        {
            var isCompatible = (collectionInfo.Category, kindOverride) switch
            {
                (CollectionCategory.List, DynamoKind.L) => true,
                (CollectionCategory.Map, DynamoKind.M) => true,
                (CollectionCategory.Set, DynamoKind.SS or DynamoKind.NS or DynamoKind.BS) =>
                    kindOverride == collectionInfo.TargetKind,
                _ => false,
            };

            if (!isCompatible)
            {
                return DiagnosticResult<TypeMappingStrategy?>.Failure(
                    DiagnosticDescriptors.IncompatibleKindOverride,
                    analysis.PropertyType.Locations.FirstOrDefault()?.CreateLocationInfo(),
                    analysis.PropertyName,
                    kindOverride.ToString(),
                    collectionInfo.TargetKind.ToString()
                );
            }
        }

        // Determine TypeName and GenericArgument for method name resolution
        var (typeName, genericArg) = collectionInfo.Category switch
        {
            CollectionCategory.List => (
                "List",
                $"<{collectionInfo.ElementType.ToDisplayString()}>"
            ),
            CollectionCategory.Map => ("Map", $"<{collectionInfo.ElementType.ToDisplayString()}>"),
            CollectionCategory.Set => collectionInfo.TargetKind switch
            {
                DynamoKind.SS => ("StringSet", ""),
                DynamoKind.NS => ("NumberSet", $"<{collectionInfo.ElementType.ToDisplayString()}>"),
                DynamoKind.BS => ("BinarySet", ""),
                _ => throw new InvalidOperationException(
                    $"Unexpected set kind: {collectionInfo.TargetKind}"
                ),
            },
            _ => throw new InvalidOperationException(
                $"Unexpected category: {collectionInfo.Category}"
            ),
        };

        // Build strategy - collections are nullable at collection level, not element level
        var strategy = CreateStrategy(typeName, analysis.Nullability, genericArg);

        // Apply Kind override if present (or use inferred kind)
        return strategy with
        {
            KindOverride = analysis.FieldOptions?.Kind ?? collectionInfo.TargetKind,
        };
    }
}
