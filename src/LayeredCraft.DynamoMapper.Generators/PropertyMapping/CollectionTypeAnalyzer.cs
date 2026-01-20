using DynamoMapper.Generator.Diagnostics;
using DynamoMapper.Generator.PropertyMapping.Models;
using DynamoMapper.Runtime;
using Microsoft.CodeAnalysis;
using WellKnownType = DynamoMapper.Generator.WellKnownTypes.WellKnownTypeData.WellKnownType;

namespace DynamoMapper.Generator.PropertyMapping;

/// <summary>
/// Analyzes types to determine if they are collections and extracts collection metadata.
/// </summary>
internal static class CollectionTypeAnalyzer
{
    internal readonly record struct ElementTypeValidationResult(
        bool IsValid,
        NestedMappingInfo? NestedMapping,
        DiagnosticInfo? Error
    );
    /// <summary>
    /// Analyzes a type to determine if it's a collection type and returns metadata about it.
    /// </summary>
    /// <param name="type">The type to analyze.</param>
    /// <param name="context">The generator context.</param>
    /// <returns>
    ///     A <see cref="CollectionInfo"/> if the type is a recognized collection, otherwise null.
    /// </returns>
    internal static CollectionInfo? Analyze(ITypeSymbol type, GeneratorContext context)
    {
        // Check for arrays first
        if (type is IArrayTypeSymbol arrayType)
        {
            var elementType = arrayType.ElementType;
            return new CollectionInfo(
                Category: CollectionCategory.List,
                ElementType: elementType,
                TargetKind: DynamoKind.L,
                KeyType: null,
                IsArray: true
            );
        }

        // For generic collections, we need a named type
        if (type is not INamedTypeSymbol namedType || !namedType.IsGenericType)
            return null;

        // Check for Dictionary<TKey, TValue> or IDictionary<TKey, TValue>
        var dictionaryType = context.WellKnownTypes.Get(WellKnownType.System_Collections_Generic_Dictionary_2);
        var iDictionaryType = context.WellKnownTypes.Get(WellKnownType.System_Collections_Generic_IDictionary_2);

        if (IsOrImplements(namedType, dictionaryType) || IsOrImplements(namedType, iDictionaryType))
        {
            // Extract generic arguments: Dictionary<TKey, TValue>
            if (namedType.TypeArguments.Length == 2)
            {
                var keyType = namedType.TypeArguments[0];
                var valueType = namedType.TypeArguments[1];

                return new CollectionInfo(
                    Category: CollectionCategory.Map,
                    ElementType: valueType,
                    TargetKind: DynamoKind.M,
                    KeyType: keyType,
                    IsArray: false
                );
            }
        }

        // Check for HashSet<T> or ISet<T>
        var hashSetType = context.WellKnownTypes.Get(WellKnownType.System_Collections_Generic_HashSet_1);
        var iSetType = context.WellKnownTypes.Get(WellKnownType.System_Collections_Generic_ISet_1);

        if (IsOrImplements(namedType, hashSetType) || IsOrImplements(namedType, iSetType))
        {
            // Extract element type: HashSet<T>
            if (namedType.TypeArguments.Length == 1)
            {
                var elementType = namedType.TypeArguments[0];
                var setKind = ResolveSetKind(elementType);

                if (setKind.HasValue)
                {
                    return new CollectionInfo(
                        Category: CollectionCategory.Set,
                        ElementType: elementType,
                        TargetKind: setKind.Value,
                        KeyType: null,
                        IsArray: false
                    );
                }
            }
        }

        // Check for List<T>, IList<T>, ICollection<T>, or IEnumerable<T>
        var listType = context.WellKnownTypes.Get(WellKnownType.System_Collections_Generic_List_1);
        var iListType = context.WellKnownTypes.Get(WellKnownType.System_Collections_Generic_IList_1);
        var iCollectionType = context.WellKnownTypes.Get(WellKnownType.System_Collections_Generic_ICollection_1);
        var iEnumerableType = context.WellKnownTypes.Get(WellKnownType.System_Collections_Generic_IEnumerable_1);

        if (IsOrImplements(namedType, listType)
            || IsOrImplements(namedType, iListType)
            || IsOrImplements(namedType, iCollectionType)
            || IsOrImplements(namedType, iEnumerableType))
        {
            // Extract element type: List<T>
            if (namedType.TypeArguments.Length == 1)
            {
                var elementType = namedType.TypeArguments[0];

                return new CollectionInfo(
                    Category: CollectionCategory.List,
                    ElementType: elementType,
                    TargetKind: DynamoKind.L,
                    KeyType: null,
                    IsArray: false
                );
            }
        }

        // Not a recognized collection type
        return null;
    }

    /// <summary>
    /// Determines if an element type is valid for use in collections.
    /// Supports primitive types and nested objects (no nested collections).
    /// </summary>
    /// <param name="elementType">The element type to validate.</param>
    /// <param name="context">The generator context.</param>
    /// <returns>True if the element type is a supported primitive, false otherwise.</returns>
    internal static bool IsValidElementType(ITypeSymbol elementType, GeneratorContext context)
    {
        var result = ValidateElementType(elementType, context);
        var isValid = result.IsValid;
        return isValid;
    }

    /// <summary>
    /// Validates an element type and returns nested mapping info if it's a nested object.
    /// </summary>
    /// <param name="elementType">The element type to validate.</param>
    /// <param name="context">The generator context.</param>
    /// <returns>A tuple of (isValid, nestedMappingInfo). nestedMappingInfo is null for primitives.</returns>
    internal static ElementTypeValidationResult ValidateElementType(
        ITypeSymbol elementType,
        GeneratorContext context
    )
    {
        var nestedContext = NestedAnalysisContext.Create(context, context.MapperRegistry);
        if (context.RootModelType is not null)
        {
            nestedContext = nestedContext.WithAncestor(context.RootModelType);
        }

        return ValidateElementType(elementType, nestedContext);
    }

    /// <summary>
    /// Validates an element type and returns nested mapping info if it's a nested object.
    /// </summary>
    /// <param name="elementType">The element type to validate.</param>
    /// <param name="nestedContext">The nested analysis context to preserve ancestor tracking.</param>
    /// <returns>A tuple of (isValid, nestedMappingInfo). nestedMappingInfo is null for primitives.</returns>
    internal static ElementTypeValidationResult ValidateElementType(
        ITypeSymbol elementType,
        NestedAnalysisContext nestedContext
    )
    {
        var context = nestedContext.Context;

        // Unwrap Nullable<T> - nullable elements are allowed
        var underlyingType = elementType;
        if (elementType is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullableType
            && nullableType.TypeArguments.Length == 1)
        {
            underlyingType = nullableType.TypeArguments[0];
        }

        // Check for supported primitive types
        switch (underlyingType.SpecialType)
        {
            case SpecialType.System_String:
            case SpecialType.System_Boolean:
            case SpecialType.System_Byte:
            case SpecialType.System_Int16:
            case SpecialType.System_Int32:
            case SpecialType.System_Int64:
            case SpecialType.System_Single:
            case SpecialType.System_Double:
            case SpecialType.System_Decimal:
            case SpecialType.System_DateTime:
                return new ElementTypeValidationResult(true, null, null);
        }

        // Check for other supported types
        if (underlyingType is INamedTypeSymbol namedType)
        {
            // Guid
            var guidType = context.WellKnownTypes.Get(WellKnownType.System_Guid);
            if (SymbolEqualityComparer.Default.Equals(namedType, guidType))
                return new ElementTypeValidationResult(true, null, null);

            // DateTimeOffset
            var dateTimeOffsetType = context.WellKnownTypes.Get(WellKnownType.System_DateTimeOffset);
            if (SymbolEqualityComparer.Default.Equals(namedType, dateTimeOffsetType))
                return new ElementTypeValidationResult(true, null, null);

            // TimeSpan
            var timeSpanType = context.WellKnownTypes.Get(WellKnownType.System_TimeSpan);
            if (SymbolEqualityComparer.Default.Equals(namedType, timeSpanType))
                return new ElementTypeValidationResult(true, null, null);

            // Enums
            if (namedType.TypeKind == TypeKind.Enum)
                return new ElementTypeValidationResult(true, null, null);
        }

        // Check for byte[] (valid for BS - Binary Set)
        if (underlyingType is IArrayTypeSymbol arrayType
            && arrayType.ElementType.SpecialType == SpecialType.System_Byte)
        {
            return new ElementTypeValidationResult(true, null, null);
        }

        // Reject nested collections
        if (Analyze(underlyingType, context) is not null)
            return new ElementTypeValidationResult(false, null, null);

        // Try to analyze as a nested object
        var nestedResult = NestedObjectTypeAnalyzer.Analyze(
            underlyingType,
            "element", // property name doesn't matter for element type analysis
            nestedContext
        );

        if (!nestedResult.IsSuccess)
            return new ElementTypeValidationResult(false, null, nestedResult.Error);

        if (nestedResult.Value is not null)
            return new ElementTypeValidationResult(true, nestedResult.Value, null);

        // Reject unsupported types
        return new ElementTypeValidationResult(false, null, null);
    }

    /// <summary>
    /// Resolves the DynamoDB set kind (SS, NS, or BS) based on the element type.
    /// </summary>
    /// <param name="elementType">The element type.</param>
    /// <returns>
    ///     The DynamoKind for the set (SS for string, NS for numbers, BS for byte[]),
    ///     or null if the type is not valid for a set.
    /// </returns>
    internal static DynamoKind? ResolveSetKind(ITypeSymbol elementType)
    {
        // Unwrap Nullable<T> if present
        var underlyingType = elementType;
        if (elementType is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullableType
            && nullableType.TypeArguments.Length == 1)
        {
            underlyingType = nullableType.TypeArguments[0];
        }

        // String → SS
        if (underlyingType.SpecialType == SpecialType.System_String)
            return DynamoKind.SS;

        // Numeric types → NS
        switch (underlyingType.SpecialType)
        {
            case SpecialType.System_Byte:
            case SpecialType.System_Int16:
            case SpecialType.System_Int32:
            case SpecialType.System_Int64:
            case SpecialType.System_Single:
            case SpecialType.System_Double:
            case SpecialType.System_Decimal:
                return DynamoKind.NS;
        }

        // byte[] → BS
        if (underlyingType is IArrayTypeSymbol arrayType
            && arrayType.ElementType.SpecialType == SpecialType.System_Byte)
        {
            return DynamoKind.BS;
        }

        // Not valid for sets
        return null;
    }

    /// <summary>
    /// Checks if a type matches or implements a generic type definition.
    /// </summary>
    private static bool IsOrImplements(INamedTypeSymbol type, INamedTypeSymbol? genericTypeDefinition)
    {
        if (genericTypeDefinition == null)
            return false;

        // Check if the type itself matches
        if (SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, genericTypeDefinition))
            return true;

        // Check if any interface matches
        return type.AllInterfaces.Any(i =>
            SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, genericTypeDefinition));
    }
}
