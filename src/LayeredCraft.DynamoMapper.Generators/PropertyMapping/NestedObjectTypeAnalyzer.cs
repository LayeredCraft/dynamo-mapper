using LayeredCraft.DynamoMapper.Generator.Diagnostics;
using LayeredCraft.DynamoMapper.Generator.Models;
using LayeredCraft.DynamoMapper.Generator.PropertyMapping.Models;
using LayeredCraft.DynamoMapper.Generator.WellKnownTypes;
using LayeredCraft.DynamoMapper.Runtime;
using LayeredCraft.SourceGeneratorTools.Types;
using Microsoft.CodeAnalysis;

namespace LayeredCraft.DynamoMapper.Generator.PropertyMapping;

/// <summary>
///     Analyzes types to determine if they are nested objects and how they should be mapped.
///     Implements the decision tree: overrides → mapper-based → inline.
/// </summary>
internal static class NestedObjectTypeAnalyzer
{
    /// <summary>
    ///     Analyzes a collection element type to determine if it's a nested object and how it should
    ///     be mapped. Unlike <see cref="Analyze" />, this method does not append a property name to the
    ///     context path, so that the caller can pre-set the correct path prefix (e.g. "Contacts") and have
    ///     field overrides like "Contacts.VerifiedAt" resolve correctly.
    /// </summary>
    /// <param name="type">The element type to analyze.</param>
    /// <param name="nestedContext">
    ///     The nested analysis context, with CurrentPath already set to the
    ///     collection property's path.
    /// </param>
    internal static DiagnosticResult<NestedMappingInfo?> AnalyzeElementType(
        ITypeSymbol type, NestedAnalysisContext nestedContext
    )
    {
        nestedContext.Context.ThrowIfCancellationRequested();

        if (!IsNestedObjectType(type, nestedContext.Context))
            return DiagnosticResult<NestedMappingInfo?>.Success(null);

        if (nestedContext.WouldCreateCycle(type))
            return DiagnosticResult<NestedMappingInfo?>.Failure(
                DiagnosticDescriptors.CycleDetectedInNestedType,
                type.Locations.FirstOrDefault()?.CreateLocationInfo(),
                "element",
                type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
            );

        if (nestedContext.HasOverridesForCurrentPath())
            return AnalyzeForInline(type, nestedContext);

        if (nestedContext.Registry.TryGetMapper(type, out var mapperReference) &&
            mapperReference != null)
        {
            var requiresTo = nestedContext.Context.HasToItemMethod;
            var requiresFrom = nestedContext.Context.HasFromItemMethod;
            if ((!requiresTo || mapperReference.HasToItemMethod) &&
                (!requiresFrom || mapperReference.HasFromItemMethod))
                return DiagnosticResult<NestedMappingInfo?>.Success(
                    new MapperBasedNesting(mapperReference)
                );
        }

        return AnalyzeForInline(type, nestedContext);
    }

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
        ITypeSymbol type, string propertyName, NestedAnalysisContext nestedContext
    )
    {
        nestedContext.Context.ThrowIfCancellationRequested();

        // Check if type is a class or struct with properties (not a primitive, enum, collection,
        // etc.)
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

        // Step 3: Check if a mapper exists for this type and supports required directions
        if (nestedContext.Registry.TryGetMapper(type, out var mapperReference) &&
            mapperReference != null)
        {
            var requiresTo = nestedContext.Context.HasToItemMethod;
            var requiresFrom = nestedContext.Context.HasFromItemMethod;

            if ((!requiresTo || mapperReference.HasToItemMethod) &&
                (!requiresFrom || mapperReference.HasFromItemMethod))
            {
                return DiagnosticResult<NestedMappingInfo?>.Success(
                    new MapperBasedNesting(mapperReference)
                );
            }
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
        var properties = GetMappableProperties(namedType, context);
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

        if (wellKnown.IsType(type, WellKnownTypeData.WellKnownType.System_DateTimeOffset))
            return true;
        if (wellKnown.IsType(type, WellKnownTypeData.WellKnownType.System_TimeSpan))
            return true;
        if (wellKnown.IsType(type, WellKnownTypeData.WellKnownType.System_Guid))
            return true;
        var streamType = wellKnown.Get(WellKnownTypeData.WellKnownType.System_IO_Stream);
        if (streamType is not null && type.IsAssignableTo(streamType, context))
            return true;

        return false;
    }

    /// <summary>
    ///     Gets the mappable properties from a type.
    /// </summary>
    private static IPropertySymbol[] GetMappableProperties(
        INamedTypeSymbol type, GeneratorContext context
    ) => PropertySymbolLookup.GetProperties(
        type,
        context.MapperOptions.IncludeBaseClassProperties,
        static (p, declaringType) =>
            !p.IsStatic && !p.IsIndexer && (p.GetMethod != null || p.SetMethod != null) &&
            !(declaringType.IsRecord && p.Name == "EqualityContract")
    );

    /// <summary>
    ///     Analyzes a type for inline code generation, recursively building property specs.
    /// </summary>
    private static DiagnosticResult<NestedMappingInfo?> AnalyzeForInline(
        ITypeSymbol type, NestedAnalysisContext nestedContext
    )
    {
        if (type is not INamedTypeSymbol namedType)
            return DiagnosticResult<NestedMappingInfo?>.Success(null);

        // Add this type to the ancestor chain for cycle detection
        var contextWithAncestor = nestedContext.WithAncestor(type);

        var properties = GetMappableProperties(namedType, nestedContext.Context);
        var propertySpecs = new List<NestedPropertySpec>();
        var diagnostics = new List<DiagnosticInfo>();

        foreach (var property in properties)
        {
            nestedContext.Context.ThrowIfCancellationRequested();

            // Check if this property should be ignored
            var ignoreOptions = nestedContext.GetIgnoreOptionsForProperty(property.Name);
            if (ignoreOptions?.Ignore is IgnoreMapping.All)
                continue;

            // Get field options for this nested property
            var fieldOptions = nestedContext.GetFieldOptionsForProperty(property.Name);

            // Analyze the property using PropertyAnalyzer
            var propertyAnalysisResult =
                PropertyAnalyzer.Analyze(property, contextWithAncestor.Context);
            if (!propertyAnalysisResult.IsSuccess)
            {
                diagnostics.Add(propertyAnalysisResult.Error!);
                continue;
            }

            var propertyAnalysis = propertyAnalysisResult.Value!;

            // Determine the DynamoDB attribute name
            var dynamoKey =
                fieldOptions?.AttributeName ??
                nestedContext.Context.MapperOptions.KeyNamingConventionConverter(property.Name);

            // Analyze the property type
            var propertyType = property.Type;
            var underlyingType = UnwrapNullable(propertyType);

            // Try to resolve as a scalar type first
            var scalarStrategy =
                TryResolveScalarStrategy(
                    underlyingType,
                    propertyType,
                    fieldOptions,
                    nestedContext.Context
                );
            if (scalarStrategy != null)
            {
                propertySpecs.Add(
                    new NestedPropertySpec(
                        property.Name,
                        dynamoKey,
                        scalarStrategy,
                        fieldOptions?.OmitIfNull,
                        fieldOptions?.OmitIfEmptyString,
                        propertyAnalysis.Nullability,
                        propertyAnalysis.HasGetter,
                        propertyAnalysis.HasSetter,
                        propertyAnalysis.IsRequired,
                        propertyAnalysis.IsInitOnly,
                        propertyAnalysis.HasDefaultValue,
                        null
                    )
                );
                continue;
            }

            // Check for collections
            var collectionInfo =
                CollectionTypeAnalyzer.Analyze(underlyingType, nestedContext.Context);
            if (collectionInfo != null)
            {
                // Include the collection property's name in the context path so that
                // element-level overrides (e.g. "Contacts.VerifiedAt") resolve correctly.
                var validation =
                    CollectionTypeAnalyzer.ValidateElementType(
                        collectionInfo.ElementType,
                        contextWithAncestor.WithPath(property.Name)
                    );

                if (validation.Error is not null)
                {
                    diagnostics.Add(validation.Error);
                    continue;
                }

                if (!validation.IsValid)
                {
                    diagnostics.Add(
                        new DiagnosticInfo(
                            DiagnosticDescriptors.UnsupportedNestedMemberType,
                            type.Locations.FirstOrDefault()?.CreateLocationInfo(),
                            nestedContext.CurrentPath,
                            property.Name,
                            propertyType.ToDisplayString()
                        )
                    );
                    continue;
                }

                // Create collection strategy — pass NestedMapping through so that
                // List<ComplexType> inside a helper method emits a NestedList strategy
                // (not a plain List strategy that would call SetList<ComplexType> at runtime).
                var collectionStrategy =
                    CreateCollectionStrategy(
                        collectionInfo,
                        propertyType,
                        fieldOptions,
                        nestedContext.Context,
                        validation.NestedMapping
                    );
                propertySpecs.Add(
                    new NestedPropertySpec(
                        property.Name,
                        dynamoKey,
                        collectionStrategy,
                        fieldOptions?.OmitIfNull,
                        fieldOptions?.OmitIfEmptyString,
                        propertyAnalysis.Nullability,
                        propertyAnalysis.HasGetter,
                        propertyAnalysis.HasSetter,
                        propertyAnalysis.IsRequired,
                        propertyAnalysis.IsInitOnly,
                        propertyAnalysis.HasDefaultValue,
                        null
                    )
                );
                continue;
            }

            // Try to analyze as a nested object (recursive)
            var nestedResult =
                Analyze(underlyingType, property.Name, contextWithAncestor.WithPath(property.Name));
            if (!nestedResult.IsSuccess)
            {
                diagnostics.Add(nestedResult.Error!);
                continue;
            }

            if (nestedResult.Value != null)
            {
                // It's a nested object
                propertySpecs.Add(
                    new NestedPropertySpec(
                        property.Name,
                        dynamoKey,
                        null,
                        fieldOptions?.OmitIfNull,
                        fieldOptions?.OmitIfEmptyString,
                        propertyAnalysis.Nullability,
                        propertyAnalysis.HasGetter,
                        propertyAnalysis.HasSetter,
                        propertyAnalysis.IsRequired,
                        propertyAnalysis.IsInitOnly,
                        propertyAnalysis.HasDefaultValue,
                        nestedResult.Value
                    )
                );
                continue;
            }

            // Unsupported type
            diagnostics.Add(
                new DiagnosticInfo(
                    DiagnosticDescriptors.UnsupportedNestedMemberType,
                    type.Locations.FirstOrDefault()?.CreateLocationInfo(),
                    nestedContext.CurrentPath,
                    property.Name,
                    propertyType.ToDisplayString()
                )
            );
        }

        // If there were errors, return the first one
        if (diagnostics.Count > 0)
            return DiagnosticResult<NestedMappingInfo?>.Failure(diagnostics[0]);

        var inlineInfo =
            new NestedInlineInfo(
                type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                new EquatableArray<NestedPropertySpec>(propertySpecs.ToArray())
            );

        return DiagnosticResult<NestedMappingInfo?>.Success(new InlineNesting(inlineInfo));
    }

    /// <summary>
    ///     Tries to resolve a scalar type mapping strategy.
    /// </summary>
    private static TypeMappingStrategy? TryResolveScalarStrategy(
        ITypeSymbol underlyingType, ITypeSymbol originalType, DynamoFieldOptions? fieldOptions,
        GeneratorContext context
    )
    {
        var isNullable =
            !SymbolEqualityComparer.Default.Equals(underlyingType, originalType) ||
            originalType.NullableAnnotation == NullableAnnotation.Annotated;
        var nullableModifier = isNullable ? "Nullable" : "";

        return underlyingType switch
        {
            { SpecialType: SpecialType.System_String } => new TypeMappingStrategy(
                "String",
                "",
                nullableModifier,
                [],
                []
            ),
            { SpecialType: SpecialType.System_Boolean } => new TypeMappingStrategy(
                "Bool",
                "",
                nullableModifier,
                [],
                []
            ),
            { SpecialType: SpecialType.System_Int32 } => new TypeMappingStrategy(
                "Int",
                "",
                nullableModifier,
                [],
                []
            ),
            { SpecialType: SpecialType.System_Int64 } => new TypeMappingStrategy(
                "Long",
                "",
                nullableModifier,
                [],
                []
            ),
            { SpecialType: SpecialType.System_Single } => new TypeMappingStrategy(
                "Float",
                "",
                nullableModifier,
                [],
                []
            ),
            { SpecialType: SpecialType.System_Double } => new TypeMappingStrategy(
                "Double",
                "",
                nullableModifier,
                [],
                []
            ),
            { SpecialType: SpecialType.System_Decimal } => new TypeMappingStrategy(
                "Decimal",
                "",
                nullableModifier,
                [],
                []
            ),
            { SpecialType: SpecialType.System_DateTime } => new TypeMappingStrategy(
                "DateTime",
                "",
                nullableModifier,
                [$"\"{fieldOptions?.Format ?? context.MapperOptions.DateTimeFormat}\""],
                [$"\"{fieldOptions?.Format ?? context.MapperOptions.DateTimeFormat}\""]
            ),
            INamedTypeSymbol t when context.WellKnownTypes.IsType(
                t,
                WellKnownTypeData.WellKnownType.System_DateTimeOffset
            ) => new TypeMappingStrategy(
                "DateTimeOffset",
                "",
                nullableModifier,
                [$"\"{fieldOptions?.Format ?? context.MapperOptions.DateTimeFormat}\""],
                [$"\"{fieldOptions?.Format ?? context.MapperOptions.DateTimeFormat}\""]
            ),
            INamedTypeSymbol t when context.WellKnownTypes.IsType(
                t,
                WellKnownTypeData.WellKnownType.System_Guid
            ) => new TypeMappingStrategy(
                "Guid",
                "",
                nullableModifier,
                [$"\"{fieldOptions?.Format ?? context.MapperOptions.GuidFormat}\""],
                [$"\"{fieldOptions?.Format ?? context.MapperOptions.GuidFormat}\""]
            ),
            INamedTypeSymbol t when context.WellKnownTypes.IsType(
                t,
                WellKnownTypeData.WellKnownType.System_TimeSpan
            ) => new TypeMappingStrategy(
                "TimeSpan",
                "",
                nullableModifier,
                [$"\"{fieldOptions?.Format ?? context.MapperOptions.TimeSpanFormat}\""],
                [$"\"{fieldOptions?.Format ?? context.MapperOptions.TimeSpanFormat}\""]
            ),
            IArrayTypeSymbol { ElementType.SpecialType: SpecialType.System_Byte } =>
                new TypeMappingStrategy("Binary", "", nullableModifier, [], []),
            INamedTypeSymbol t when
                context.WellKnownTypes.Get(WellKnownTypeData.WellKnownType.System_IO_Stream) is
                    { } streamType &&
                t.IsAssignableTo(streamType, context) => new TypeMappingStrategy(
                    "Stream",
                    "",
                    nullableModifier,
                    [],
                    []
                ),
            INamedTypeSymbol { TypeKind: TypeKind.Enum } enumType => CreateEnumStrategy(
                enumType,
                isNullable,
                fieldOptions,
                context
            ),
            _ => null
        };
    }

    /// <summary>
    ///     Creates an enum type mapping strategy.
    /// </summary>
    private static TypeMappingStrategy CreateEnumStrategy(
        INamedTypeSymbol enumType, bool isNullable, DynamoFieldOptions? fieldOptions,
        GeneratorContext context
    )
    {
        var enumName = enumType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var format = fieldOptions?.Format ?? context.MapperOptions.EnumFormat;
        var enumFormat = $"\"{format}\"";
        var nullableModifier = isNullable ? "Nullable" : "";

        string[] fromArgs =
            isNullable ? [enumFormat] : [$"{enumName}.{enumType.MemberNames.First()}", enumFormat];

        return new TypeMappingStrategy(
            "Enum",
            $"<{enumName}>",
            nullableModifier,
            fromArgs,
            [enumFormat]
        );
    }

    /// <summary>
    ///     Creates a collection type mapping strategy.
    ///     When <paramref name="elementNestedMapping"/> is non-null the element type is a complex
    ///     object, so a "NestedList" / "NestedMap" strategy is returned instead of the plain scalar
    ///     variant — preventing helper methods from calling SetList/GetList on complex types.
    /// </summary>
    private static TypeMappingStrategy CreateCollectionStrategy(
        CollectionInfo collectionInfo, ITypeSymbol originalType, DynamoFieldOptions? fieldOptions,
        GeneratorContext context, NestedMappingInfo? elementNestedMapping = null
    )
    {
        var isNullable = originalType.NullableAnnotation == NullableAnnotation.Annotated;
        var nullableModifier = isNullable ? "Nullable" : "";

        // When the element type is a complex object, mirror TypeMappingStrategyResolver's
        // CreateNestedCollectionStrategy so the renderers emit the correct Select(x => ...) code.
        if (elementNestedMapping is not null)
        {
            var nestedTypeName =
                collectionInfo.Category switch
                {
                    CollectionCategory.List => "NestedList",
                    CollectionCategory.Map => "NestedMap",
                    _ => throw new InvalidOperationException(
                        $"Unexpected category for nested collection: {collectionInfo.Category}"
                    ),
                };

            return new TypeMappingStrategy(
                nestedTypeName,
                $"<{collectionInfo.ElementType.ToDisplayString()}>",
                nullableModifier,
                [],
                [],
                collectionInfo.TargetKind,
                elementNestedMapping,
                collectionInfo with { ElementNestedMapping = elementNestedMapping }
            );
        }

        var (typeName, genericArg) =
            collectionInfo.Category switch
            {
                CollectionCategory.List => ("List",
                    $"<{collectionInfo.ElementType.ToDisplayString()}>"),
                CollectionCategory.Map => ("Map",
                    $"<{collectionInfo.ElementType.ToDisplayString()}>"),
                CollectionCategory.Set => collectionInfo.TargetKind switch
                {
                    DynamoKind.SS => ("StringSet", ""),
                    DynamoKind.NS => ("NumberSet",
                        $"<{collectionInfo.ElementType.ToDisplayString()}>"),
                    DynamoKind.BS => ("BinarySet", ""),
                    _ => throw new InvalidOperationException(
                        $"Unexpected set kind: {collectionInfo.TargetKind}"
                    ),
                },
                _ => throw new InvalidOperationException(
                    $"Unexpected category: {collectionInfo.Category}"
                ),
            };

        var (fromArgs, toArgs) =
            GetCollectionElementTypeSpecificArgs(collectionInfo.ElementType, fieldOptions, context);

        return new TypeMappingStrategy(
            typeName,
            genericArg,
            nullableModifier,
            fromArgs,
            toArgs,
            KindOverride: collectionInfo.TargetKind
        );
    }

    private static (string[] FromArgs, string[] ToArgs) GetCollectionElementTypeSpecificArgs(
        ITypeSymbol elementType, DynamoFieldOptions? fieldOptions, GeneratorContext context
    )
    {
        var underlyingType = UnwrapNullable(elementType);

        return underlyingType switch
        {
            { SpecialType: SpecialType.System_DateTime } => CreateCollectionFormatArgs(
                fieldOptions?.Format ?? context.MapperOptions.DateTimeFormat
            ),
            INamedTypeSymbol t when context.WellKnownTypes.IsType(
                t,
                WellKnownTypeData.WellKnownType.System_DateTimeOffset
            ) => CreateCollectionFormatArgs(
                fieldOptions?.Format ?? context.MapperOptions.DateTimeFormat
            ),
            INamedTypeSymbol t when context.WellKnownTypes.IsType(
                t,
                WellKnownTypeData.WellKnownType.System_Guid
            ) => CreateCollectionFormatArgs(
                fieldOptions?.Format ?? context.MapperOptions.GuidFormat
            ),
            INamedTypeSymbol t when context.WellKnownTypes.IsType(
                t,
                WellKnownTypeData.WellKnownType.System_TimeSpan
            ) => CreateCollectionFormatArgs(
                fieldOptions?.Format ?? context.MapperOptions.TimeSpanFormat
            ),
            INamedTypeSymbol { TypeKind: TypeKind.Enum } => CreateCollectionFormatArgs(
                fieldOptions?.Format ?? context.MapperOptions.EnumFormat
            ),
            _ => ([], []),
        };
    }

    private static (string[] FromArgs, string[] ToArgs) CreateCollectionFormatArgs(string format)
    {
        var arg = $"\"{format}\"";
        return ([arg], [arg]);
    }

    /// <summary>
    ///     Unwraps Nullable{T} to get the underlying type.
    /// </summary>
    private static ITypeSymbol UnwrapNullable(ITypeSymbol type)
    {
        if (type is INamedTypeSymbol
            {
                OriginalDefinition.SpecialType: SpecialType.System_Nullable_T,
            } nullableType && nullableType.TypeArguments.Length == 1)
        {
            return nullableType.TypeArguments[0];
        }

        return type;
    }
}
