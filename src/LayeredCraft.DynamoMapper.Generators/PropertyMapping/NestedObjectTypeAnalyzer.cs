using DynamoMapper.Generator.Diagnostics;
using DynamoMapper.Generator.Models;
using DynamoMapper.Generator.PropertyMapping.Models;
using LayeredCraft.SourceGeneratorTools.Types;
using Microsoft.CodeAnalysis;

namespace DynamoMapper.Generator.PropertyMapping;

/// <summary>
///     Analyzes types to determine if they are nested objects and how they should be mapped.
///     Implements the decision tree: overrides → mapper-based → inline.
/// </summary>
internal static class NestedObjectTypeAnalyzer
{
    /// <summary>
    ///     Analyzes a type to determine if it's a nested object and how it should be mapped.
    /// </summary>
    /// <param name="type">The type to analyze.</param>
    /// <param name="propertyName">The property name (for diagnostics).</param>
    /// <param name="nestedContext">The nested analysis context.</param>
    /// <returns>
    ///     A diagnostic result containing the nested mapping info, or null if not a nested object,
    ///     or a failure for cycles/unsupported types.
    /// </returns>
    internal static DiagnosticResult<NestedMappingInfo?> Analyze(
        ITypeSymbol type,
        string propertyName,
        NestedAnalysisContext nestedContext
    )
    {
        nestedContext.Context.ThrowIfCancellationRequested();

        // Check if type is a class or struct with properties (not a primitive, enum, collection, etc.)
        if (!IsNestedObjectType(type, nestedContext.Context))
            return DiagnosticResult<NestedMappingInfo?>.Success(null);

        // Step 1: Check for cycle - would analyzing this type create a circular reference?
        if (nestedContext.WouldCreateCycle(type))
        {
            return DiagnosticResult<NestedMappingInfo?>.Failure(
                DiagnosticDescriptors.CycleDetectedInNestedType,
                type.Locations.FirstOrDefault()?.CreateLocationInfo(),
                propertyName,
                type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
            );
        }

        // Step 2: Check for dot-notation overrides for this path
        // Any override triggers inline generation (skip mapper lookup)
        var contextWithPath = nestedContext.WithPath(propertyName);
        if (contextWithPath.HasOverridesForCurrentPath())
        {
            return AnalyzeForInline(type, contextWithPath);
        }

        // Step 3: Check if a mapper exists for this type
        if (nestedContext.Registry.TryGetMapper(type, out var mapperReference) && mapperReference != null)
        {
            return DiagnosticResult<NestedMappingInfo?>.Success(
                new MapperBasedNesting(mapperReference)
            );
        }

        // Step 4: Default to inline generation
        return AnalyzeForInline(type, contextWithPath);
    }

    /// <summary>
    ///     Determines if a type should be treated as a nested object.
    /// </summary>
    private static bool IsNestedObjectType(ITypeSymbol type, GeneratorContext context)
    {
        // Must be a named type
        if (type is not INamedTypeSymbol namedType)
            return false;

        // Exclude primitives and special types
        if (type.SpecialType != SpecialType.None)
            return false;

        // Exclude enums
        if (namedType.TypeKind == TypeKind.Enum)
            return false;

        // Exclude collections (handled separately)
        if (CollectionTypeAnalyzer.Analyze(type, context) is not null)
            return false;

        // Must be a class, struct, or record (not interface, delegate, etc.)
        if (namedType.TypeKind is not (TypeKind.Class or TypeKind.Struct))
            return false;

        // Exclude well-known types that aren't nested objects
        if (IsWellKnownNonNestedType(namedType, context))
            return false;

        // Check it has mappable properties
        var properties = GetMappableProperties(namedType);
        if (properties.Length == 0)
            return false;

        return true;
    }

    /// <summary>
    ///     Checks if a type is a well-known type that shouldn't be treated as a nested object.
    /// </summary>
    private static bool IsWellKnownNonNestedType(INamedTypeSymbol type, GeneratorContext context)
    {
        // DateTime is a special type (SpecialType.System_DateTime), already excluded above
        // DateTimeOffset, TimeSpan, Guid are well-known scalar types, not nested objects
        var wellKnown = context.WellKnownTypes;

        if (wellKnown.IsType(type, WellKnownTypes.WellKnownTypeData.WellKnownType.System_DateTimeOffset))
            return true;
        if (wellKnown.IsType(type, WellKnownTypes.WellKnownTypeData.WellKnownType.System_TimeSpan))
            return true;
        if (wellKnown.IsType(type, WellKnownTypes.WellKnownTypeData.WellKnownType.System_Guid))
            return true;

        return false;
    }

    /// <summary>
    ///     Gets the mappable properties from a type.
    /// </summary>
    private static IPropertySymbol[] GetMappableProperties(INamedTypeSymbol type)
    {
        return type
            .GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => !p.IsStatic && !p.IsIndexer && (p.GetMethod != null || p.SetMethod != null))
            .Where(p => !(type.IsRecord && p.Name == "EqualityContract"))
            .ToArray();
    }

    /// <summary>
    ///     Analyzes a type for inline code generation, recursively building property specs.
    /// </summary>
    private static DiagnosticResult<NestedMappingInfo?> AnalyzeForInline(
        ITypeSymbol type,
        NestedAnalysisContext nestedContext
    )
    {
        if (type is not INamedTypeSymbol namedType)
            return DiagnosticResult<NestedMappingInfo?>.Success(null);

        // Add this type to the ancestor chain for cycle detection
        var contextWithAncestor = nestedContext.WithAncestor(type);

        var properties = GetMappableProperties(namedType);
        var propertySpecs = new List<NestedPropertySpec>();
        var diagnostics = new List<DiagnosticInfo>();

        foreach (var property in properties)
        {
            nestedContext.Context.ThrowIfCancellationRequested();

            // Check if this property should be ignored
            var ignoreOptions = nestedContext.GetIgnoreOptionsForProperty(property.Name);
            if (ignoreOptions?.Ignore is Runtime.IgnoreMapping.All)
                continue;

            // Get field options for this nested property
            var fieldOptions = nestedContext.GetFieldOptionsForProperty(property.Name);

            // Determine the DynamoDB attribute name
            var dynamoKey = fieldOptions?.AttributeName
                ?? nestedContext.Context.MapperOptions.KeyNamingConventionConverter(property.Name);

            // Analyze the property type
            var propertyType = property.Type;
            var underlyingType = UnwrapNullable(propertyType);

            // Try to resolve as a scalar type first
            var scalarStrategy = TryResolveScalarStrategy(underlyingType, propertyType, fieldOptions, nestedContext.Context);
            if (scalarStrategy != null)
            {
                propertySpecs.Add(new NestedPropertySpec(
                    property.Name,
                    dynamoKey,
                    scalarStrategy,
                    NestedMapping: null
                ));
                continue;
            }

            // Check for collections
            var collectionInfo = CollectionTypeAnalyzer.Analyze(underlyingType, nestedContext.Context);
            if (collectionInfo != null)
            {
                // For now, only support primitive element types in nested collections
                if (!CollectionTypeAnalyzer.IsValidElementType(collectionInfo.ElementType, nestedContext.Context))
                {
                    diagnostics.Add(new DiagnosticInfo(
                        DiagnosticDescriptors.UnsupportedNestedMemberType,
                        type.Locations.FirstOrDefault()?.CreateLocationInfo(),
                        nestedContext.CurrentPath,
                        property.Name,
                        propertyType.ToDisplayString()
                    ));
                    continue;
                }

                // Create collection strategy (simplified for nested objects)
                var collectionStrategy = CreateCollectionStrategy(collectionInfo, propertyType);
                propertySpecs.Add(new NestedPropertySpec(
                    property.Name,
                    dynamoKey,
                    collectionStrategy,
                    NestedMapping: null
                ));
                continue;
            }

            // Try to analyze as a nested object (recursive)
            var nestedResult = Analyze(underlyingType, property.Name, contextWithAncestor.WithPath(property.Name));
            if (!nestedResult.IsSuccess)
            {
                diagnostics.Add(nestedResult.Error!);
                continue;
            }

            if (nestedResult.Value != null)
            {
                // It's a nested object
                propertySpecs.Add(new NestedPropertySpec(
                    property.Name,
                    dynamoKey,
                    Strategy: null,
                    NestedMapping: nestedResult.Value
                ));
                continue;
            }

            // Unsupported type
            diagnostics.Add(new DiagnosticInfo(
                DiagnosticDescriptors.UnsupportedNestedMemberType,
                type.Locations.FirstOrDefault()?.CreateLocationInfo(),
                nestedContext.CurrentPath,
                property.Name,
                propertyType.ToDisplayString()
            ));
        }

        // If there were errors, return the first one
        if (diagnostics.Count > 0)
            return DiagnosticResult<NestedMappingInfo?>.Failure(diagnostics[0]);

        var inlineInfo = new NestedInlineInfo(
            type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            new EquatableArray<NestedPropertySpec>(propertySpecs.ToArray())
        );

        return DiagnosticResult<NestedMappingInfo?>.Success(new InlineNesting(inlineInfo));
    }

    /// <summary>
    ///     Tries to resolve a scalar type mapping strategy.
    /// </summary>
    private static TypeMappingStrategy? TryResolveScalarStrategy(
        ITypeSymbol underlyingType,
        ITypeSymbol originalType,
        DynamoFieldOptions? fieldOptions,
        GeneratorContext context
    )
    {
        var isNullable = !SymbolEqualityComparer.Default.Equals(underlyingType, originalType)
            || originalType.NullableAnnotation == NullableAnnotation.Annotated;
        var nullableModifier = isNullable ? "Nullable" : "";

        return underlyingType switch
        {
            { SpecialType: SpecialType.System_String } => new TypeMappingStrategy(
                "String", "", nullableModifier, [], []
            ),
            { SpecialType: SpecialType.System_Boolean } => new TypeMappingStrategy(
                "Bool", "", nullableModifier, [], []
            ),
            { SpecialType: SpecialType.System_Int32 } => new TypeMappingStrategy(
                "Int", "", nullableModifier, [], []
            ),
            { SpecialType: SpecialType.System_Int64 } => new TypeMappingStrategy(
                "Long", "", nullableModifier, [], []
            ),
            { SpecialType: SpecialType.System_Single } => new TypeMappingStrategy(
                "Float", "", nullableModifier, [], []
            ),
            { SpecialType: SpecialType.System_Double } => new TypeMappingStrategy(
                "Double", "", nullableModifier, [], []
            ),
            { SpecialType: SpecialType.System_Decimal } => new TypeMappingStrategy(
                "Decimal", "", nullableModifier, [], []
            ),
            { SpecialType: SpecialType.System_DateTime } => new TypeMappingStrategy(
                "DateTime", "", nullableModifier,
                [$"\"{fieldOptions?.Format ?? context.MapperOptions.DateTimeFormat}\""],
                [$"\"{fieldOptions?.Format ?? context.MapperOptions.DateTimeFormat}\""]
            ),
            INamedTypeSymbol t when context.WellKnownTypes.IsType(
                t, WellKnownTypes.WellKnownTypeData.WellKnownType.System_DateTimeOffset
            ) => new TypeMappingStrategy(
                "DateTimeOffset", "", nullableModifier,
                [$"\"{fieldOptions?.Format ?? context.MapperOptions.DateTimeFormat}\""],
                [$"\"{fieldOptions?.Format ?? context.MapperOptions.DateTimeFormat}\""]
            ),
            INamedTypeSymbol t when context.WellKnownTypes.IsType(
                t, WellKnownTypes.WellKnownTypeData.WellKnownType.System_Guid
            ) => new TypeMappingStrategy(
                "Guid", "", nullableModifier,
                [$"\"{fieldOptions?.Format ?? context.MapperOptions.GuidFormat}\""],
                [$"\"{fieldOptions?.Format ?? context.MapperOptions.GuidFormat}\""]
            ),
            INamedTypeSymbol t when context.WellKnownTypes.IsType(
                t, WellKnownTypes.WellKnownTypeData.WellKnownType.System_TimeSpan
            ) => new TypeMappingStrategy(
                "TimeSpan", "", nullableModifier,
                [$"\"{fieldOptions?.Format ?? context.MapperOptions.TimeSpanFormat}\""],
                [$"\"{fieldOptions?.Format ?? context.MapperOptions.TimeSpanFormat}\""]
            ),
            INamedTypeSymbol { TypeKind: TypeKind.Enum } enumType => CreateEnumStrategy(
                enumType, isNullable, fieldOptions, context
            ),
            _ => null
        };
    }

    /// <summary>
    ///     Creates an enum type mapping strategy.
    /// </summary>
    private static TypeMappingStrategy CreateEnumStrategy(
        INamedTypeSymbol enumType,
        bool isNullable,
        DynamoFieldOptions? fieldOptions,
        GeneratorContext context
    )
    {
        var enumName = enumType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var format = fieldOptions?.Format ?? context.MapperOptions.EnumFormat;
        var enumFormat = $"\"{format}\"";
        var nullableModifier = isNullable ? "Nullable" : "";

        string[] fromArgs = isNullable
            ? [enumFormat]
            : [$"{enumName}.{enumType.MemberNames.First()}", enumFormat];

        return new TypeMappingStrategy("Enum", $"<{enumName}>", nullableModifier, fromArgs, [enumFormat]);
    }

    /// <summary>
    ///     Creates a collection type mapping strategy.
    /// </summary>
    private static TypeMappingStrategy CreateCollectionStrategy(
        CollectionInfo collectionInfo,
        ITypeSymbol originalType
    )
    {
        var isNullable = originalType.NullableAnnotation == NullableAnnotation.Annotated;
        var nullableModifier = isNullable ? "Nullable" : "";

        var (typeName, genericArg) = collectionInfo.Category switch
        {
            CollectionCategory.List => ("List", $"<{collectionInfo.ElementType.ToDisplayString()}>"),
            CollectionCategory.Map => ("Map", $"<{collectionInfo.ElementType.ToDisplayString()}>"),
            CollectionCategory.Set => collectionInfo.TargetKind switch
            {
                Runtime.DynamoKind.SS => ("StringSet", ""),
                Runtime.DynamoKind.NS => ("NumberSet", $"<{collectionInfo.ElementType.ToDisplayString()}>"),
                Runtime.DynamoKind.BS => ("BinarySet", ""),
                _ => throw new InvalidOperationException($"Unexpected set kind: {collectionInfo.TargetKind}")
            },
            _ => throw new InvalidOperationException($"Unexpected category: {collectionInfo.Category}")
        };

        return new TypeMappingStrategy(
            typeName, genericArg, nullableModifier, [], [],
            KindOverride: collectionInfo.TargetKind
        );
    }

    /// <summary>
    ///     Unwraps Nullable{T} to get the underlying type.
    /// </summary>
    private static ITypeSymbol UnwrapNullable(ITypeSymbol type)
    {
        if (type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullableType
            && nullableType.TypeArguments.Length == 1)
        {
            return nullableType.TypeArguments[0];
        }
        return type;
    }
}
