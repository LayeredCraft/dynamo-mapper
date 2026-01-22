using System.Diagnostics;
using System.Text;
using DynamoMapper.Generator.ConstructorMapping.Models;
using DynamoMapper.Generator.Models;
using DynamoMapper.Generator.PropertyMapping.Models;
using Microsoft.CodeAnalysis;

namespace DynamoMapper.Generator.PropertyMapping;

/// <summary>
///     Renders property mapping specifications into PropertyInfo. Generates the final
///     FromAssignment and ToAssignments strings for the Scriban template.
/// </summary>
internal static class PropertyMappingCodeRenderer
{
    /// <summary>Renders a property mapping specification into PropertyInfo.</summary>
    internal static PropertyInfo Render(
        PropertyMappingSpec spec,
        PropertyAnalysis analysis,
        string modelVarName,
        int index,
        InitializationMethod initMethod,
        GeneratorContext context
    )
    {
        // Check if this is a nested object - requires special handling
        var isNestedObject = spec.TypeStrategy?.NestedMapping is not null
            && spec.TypeStrategy.CollectionInfo is null;

        // Check if this is a nested collection (List<NestedType> or Dictionary<string, NestedType>)
        var isNestedCollection = spec.TypeStrategy?.CollectionInfo?.ElementNestedMapping is not null;

        // If this property is a constructor parameter, render as constructor argument
        if (initMethod == InitializationMethod.ConstructorParameter && context.HasFromItemMethod)
        {
            // Nested objects/collections in constructors are not yet supported
            var constructorArg = spec.FromItemMethod is not null && !isNestedObject && !isNestedCollection
                ? RenderConstructorArgument(spec, analysis, context)
                : null;

            // Constructor parameters still need ToAssignments for serialization
            string? toAssignment = null;
            if (context.HasToItemMethod && analysis.HasGetter && spec.ToItemMethod is not null)
            {
                if (isNestedObject)
                    toAssignment = RenderNestedObjectToAssignment(spec, analysis, context);
                else if (isNestedCollection)
                    toAssignment = RenderNestedCollectionToAssignment(spec, analysis, context);
                else
                    toAssignment = RenderToAssignment(spec);
            }

            return new PropertyInfo(null, null, toAssignment, constructorArg);
        }

        // Handle nested collections specially (List<Address>, Dictionary<string, Address>)
        if (isNestedCollection)
        {
            return RenderNestedCollectionProperty(spec, analysis, modelVarName, initMethod, context);
        }

        // Handle nested objects specially
        if (isNestedObject)
        {
            return RenderNestedObjectProperty(spec, analysis, modelVarName, initMethod, context);
        }

        // Determine if we should use regular assignment vs init assignment
        var useRegularAssignment =
            spec.FromItemMethod is { IsCustomMethod: false }
            && analysis is { IsRequired: false, IsInitOnly: false, HasDefaultValue: true };

        // Use init syntax for InitSyntax mode, regular assignment for PostConstruction
        var useInitSyntax = initMethod == InitializationMethod.InitSyntax;

        // FromItem requires both: setter on property AND FromItem method exists
        var fromAssignment =
            context.HasFromItemMethod
            && analysis.HasSetter
            && spec.FromItemMethod is not null
            && (!useInitSyntax || useRegularAssignment)
                ? RenderFromAssignment(spec, modelVarName, analysis, index, context)
                : null;

        var fromInitAssignment =
            context.HasFromItemMethod
            && analysis.HasSetter
            && spec.FromItemMethod is not null
            && useInitSyntax
            && !useRegularAssignment
                ? RenderFromInitAssignment(spec, analysis, context)
                : null;

        // ToItem requires both: getter on property AND ToItem method exists
        var toAssignments =
            context.HasToItemMethod && analysis.HasGetter && spec.ToItemMethod is not null
                ? RenderToAssignment(spec)
                : null;

        return new PropertyInfo(fromAssignment, fromInitAssignment, toAssignments, null);
    }

    /// <summary>
    ///     Renders a nested object property into PropertyInfo.
    /// </summary>
    private static PropertyInfo RenderNestedObjectProperty(
        PropertyMappingSpec spec,
        PropertyAnalysis analysis,
        string modelVarName,
        InitializationMethod initMethod,
        GeneratorContext context
    )
    {
        // Use init syntax for InitSyntax mode, regular assignment for PostConstruction
        var useInitSyntax = initMethod == InitializationMethod.InitSyntax;

        // FromItem for nested objects
        var fromAssignment =
            context.HasFromItemMethod
            && analysis.HasSetter
            && spec.FromItemMethod is not null
            && !useInitSyntax
                ? RenderNestedObjectFromAssignment(spec, modelVarName, analysis, context)
                : null;

        var fromInitAssignment =
            context.HasFromItemMethod
            && analysis.HasSetter
            && spec.FromItemMethod is not null
            && useInitSyntax
                ? RenderNestedObjectFromInitAssignment(spec, analysis, context)
                : null;

        // ToItem for nested objects
        var toAssignments =
            context.HasToItemMethod && analysis.HasGetter && spec.ToItemMethod is not null
                ? RenderNestedObjectToAssignment(spec, analysis, context)
                : null;

        return new PropertyInfo(fromAssignment, fromInitAssignment, toAssignments, null);
    }

    /// <summary>
    ///     Renders the FromAssignment string for deserialization. Format: PropertyName =
    ///     paramName.MethodName&lt;Generic&gt;(args), OR PropertyName = CustomMethodName(args), for
    ///     custom methods
    /// </summary>
    private static string RenderFromInitAssignment(
        PropertyMappingSpec spec,
        PropertyAnalysis analysis,
        GeneratorContext context
    )
    {
        Debug.Assert(spec.FromItemMethod is not null, "FromItemMethod should not be null");
        Debug.Assert(
            spec.FromItemMethod!.IsCustomMethod || spec.TypeStrategy is not null,
            "TypeStrategy should not be null for standard methods"
        );

        var args = string.Join(", ", spec.FromItemMethod.Arguments.Select(a => a.Value));

        var methodCall = spec.FromItemMethod.IsCustomMethod
            ? $"{spec.FromItemMethod.MethodName}({args})" // Custom: MethodName(item)
            : $"{context.MapperOptions.FromMethodParameterName}.{spec.FromItemMethod.MethodName}{spec.TypeStrategy!.GenericArgument}({args})"; // Standard: item.GetXxx<T>(args)

        // For array properties, append .ToArray() to convert the List to an array
        // GetList returns List<T>, but if the property is T[], we need to convert it
        var isArrayProperty = analysis.PropertyType.TypeKind == TypeKind.Array;
        if (isArrayProperty && !spec.FromItemMethod.IsCustomMethod)
        {
            methodCall += ".ToArray()";
        }

        return $"{spec.PropertyName} = {methodCall},";
    }

    private static string RenderFromAssignment(
        PropertyMappingSpec spec,
        string modelVarName,
        PropertyAnalysis analysis,
        int index,
        GeneratorContext context
    )
    {
        Debug.Assert(spec.FromItemMethod is not null, "FromItemMethod should not be null");
        Debug.Assert(
            spec.FromItemMethod!.IsCustomMethod || spec.TypeStrategy is not null,
            "TypeStrategy should not be null for standard methods"
        );

        var argsList = spec.FromItemMethod.Arguments.Select(a => a.Value).ToList();

        // For NULLABLE Enum methods with format, format comes before out parameter (position 1)
        // For non-nullable enums, out comes first (after key), then defaultValue, then format
        // For DateTime/DateTimeOffset/TimeSpan, out comes before format
        var isNullableEnumWithFormat =
            spec.TypeStrategy?.TypeName == "Enum"
            && spec.TypeStrategy?.NullableModifier == "Nullable"
            && argsList.Any(a => a.Contains("format:"));
        var outPosition = isNullableEnumWithFormat ? 2 : 1;

        argsList.Insert(outPosition, $"out var var{index}");
        var args = string.Join(", ", argsList);

        var methodCall =
            $"Try{spec.FromItemMethod.MethodName}{spec.TypeStrategy!.GenericArgument}({args})";

        // For array properties, append .ToArray() to convert the List to an array
        // GetList returns List<T>, but if the property is T[], we need to convert it
        var isArrayProperty = analysis.PropertyType.TypeKind == TypeKind.Array;
        var toArray =
            isArrayProperty && !spec.FromItemMethod.IsCustomMethod ? ".ToArray()" : string.Empty;

        return $"if ({context.MapperOptions.FromMethodParameterName}.{methodCall}) {modelVarName}.{spec.PropertyName} = var{index}!{toArray};";
    }

    /// <summary>
    ///     Renders the ToAssignment string for serialization. Format: .MethodName&lt;Generic&gt;(args)
    ///     Custom ToMethods are rendered as .Set("key", CustomMethod(source))
    /// </summary>
    private static string RenderToAssignment(PropertyMappingSpec spec)
    {
        Debug.Assert(spec.ToItemMethod is not null, "ToItemMethod should not be null");
        Debug.Assert(
            spec.ToItemMethod!.IsCustomMethod || spec.TypeStrategy is not null,
            "TypeStrategy should not be null for standard methods"
        );

        var args = string.Join(", ", spec.ToItemMethod.Arguments.Select(a => a.Value));

        var methodCall = spec.ToItemMethod.IsCustomMethod
            ? $".{spec.ToItemMethod.MethodName}({args})" // Custom: .Set("key",
                                                         // CustomMethod(source))
            : $".{spec.ToItemMethod.MethodName}{spec.TypeStrategy!.GenericArgument}({args})"; // Standard: .SetXxx<T>(args)

        return methodCall;
    }

    /// <summary>
    ///     Renders a constructor argument expression. This is similar to FromInitAssignment but
    ///     without the "PropertyName = " prefix, as it's used as a constructor parameter value.
    /// </summary>
    private static string RenderConstructorArgument(
        PropertyMappingSpec spec,
        PropertyAnalysis analysis,
        GeneratorContext context
    )
    {
        Debug.Assert(spec.FromItemMethod is not null, "FromItemMethod should not be null");
        Debug.Assert(
            spec.FromItemMethod!.IsCustomMethod || spec.TypeStrategy is not null,
            "TypeStrategy should not be null for standard methods"
        );

        var args = string.Join(", ", spec.FromItemMethod.Arguments.Select(a => a.Value));

        var methodCall = spec.FromItemMethod.IsCustomMethod
            ? $"{spec.FromItemMethod.MethodName}({args})" // Custom: MethodName(item)
            : $"{context.MapperOptions.FromMethodParameterName}.{spec.FromItemMethod.MethodName}{spec.TypeStrategy!.GenericArgument}({args})"; // Standard: item.GetXxx<T>(args)

        // For array properties, append .ToArray() to convert the List to an array
        var isArrayProperty = analysis.PropertyType.TypeKind == TypeKind.Array;
        if (isArrayProperty && !spec.FromItemMethod.IsCustomMethod)
        {
            methodCall += ".ToArray()";
        }

        return methodCall;
    }

    #region Nested Object Rendering

    /// <summary>
    ///     Renders the ToItem code for a nested object.
    /// </summary>
    private static string RenderNestedObjectToAssignment(
        PropertyMappingSpec spec,
        PropertyAnalysis analysis,
        GeneratorContext context
    )
    {
        var nestedMapping = spec.TypeStrategy!.NestedMapping!;
        var paramName = context.MapperOptions.ToMethodParameterName;
        var isNullable = spec.TypeStrategy.NullableModifier == "Nullable";

        return nestedMapping switch
        {
            MapperBasedNesting mapperBased => RenderMapperBasedToAssignment(
                spec, paramName, isNullable, mapperBased.Mapper
            ),
            InlineNesting inline => RenderInlineToAssignment(
                spec, paramName, isNullable, inline.Info, context
            ),
            _ => throw new InvalidOperationException($"Unknown nested mapping type: {nestedMapping.GetType()}")
        };
    }

    /// <summary>
    ///     Renders ToItem code for mapper-based nested objects.
    ///     Output: .Set("key", source.Prop is null ? new AttributeValue { NULL = true } : new AttributeValue { M = MapperName.ToItem(source.Prop) })
    /// </summary>
    private static string RenderMapperBasedToAssignment(
        PropertyMappingSpec spec,
        string paramName,
        bool isNullable,
        MapperReference mapper
    )
    {
        var propAccess = $"{paramName}.{spec.PropertyName}";

        if (isNullable)
        {
            return $".Set(\"{spec.Key}\", {propAccess} is null ? new AttributeValue {{ NULL = true }} : new AttributeValue {{ M = {mapper.MapperFullyQualifiedName}.ToItem({propAccess}) }})";
        }

        return $".Set(\"{spec.Key}\", new AttributeValue {{ M = {mapper.MapperFullyQualifiedName}.ToItem({propAccess}) }})";
    }

    /// <summary>
    ///     Renders ToItem code for inline nested objects.
    ///     Output: .Set("key", source.Prop is null ? new AttributeValue { NULL = true } : new AttributeValue { M = new Dictionary...().SetXxx()... })
    /// </summary>
    private static string RenderInlineToAssignment(
        PropertyMappingSpec spec,
        string paramName,
        bool isNullable,
        NestedInlineInfo inlineInfo,
        GeneratorContext context
    )
    {
        var propAccess = $"{paramName}.{spec.PropertyName}";
        var inlineCode = RenderInlineNestedToItem(propAccess, inlineInfo, context);

        if (isNullable)
        {
            return $".Set(\"{spec.Key}\", {propAccess} is null ? new AttributeValue {{ NULL = true }} : new AttributeValue {{ M = {inlineCode} }})";
        }

        return $".Set(\"{spec.Key}\", new AttributeValue {{ M = {inlineCode} }})";
    }

    /// <summary>
    ///     Renders the inline Dictionary creation code for nested objects.
    /// </summary>
    private static string RenderInlineNestedToItem(
        string sourcePrefix,
        NestedInlineInfo inlineInfo,
        GeneratorContext context
    )
    {
        var sb = new StringBuilder();
        sb.Append("new Dictionary<string, AttributeValue>()");

        foreach (var prop in inlineInfo.Properties)
        {
            if (prop.NestedMapping is not null)
            {
                // Recursive nested object
                var nestedSourcePrefix = $"{sourcePrefix}.{prop.PropertyName}";
                var nestedCode = prop.NestedMapping switch
                {
                    MapperBasedNesting mapperBased =>
                        $"new AttributeValue {{ M = {mapperBased.Mapper.MapperFullyQualifiedName}.ToItem({nestedSourcePrefix}) }}",
                    InlineNesting inline =>
                        $"new AttributeValue {{ M = {RenderInlineNestedToItem(nestedSourcePrefix, inline.Info, context)} }}",
                    _ => throw new InvalidOperationException($"Unknown nested mapping type")
                };
                sb.Append($".Set(\"{prop.DynamoKey}\", {nestedSourcePrefix} is null ? new AttributeValue {{ NULL = true }} : {nestedCode})");
            }
            else if (prop.Strategy is not null)
            {
                // Scalar property
                var setMethod = $"Set{prop.Strategy.TypeName}";
                var genericArg = prop.Strategy.GenericArgument;
                var propValue = $"{sourcePrefix}.{prop.PropertyName}";

                // Build type-specific args
                var typeArgs = prop.Strategy.ToTypeSpecificArgs.Length > 0
                    ? ", " + string.Join(", ", prop.Strategy.ToTypeSpecificArgs)
                    : "";

                // Omit flags - use mapper defaults
                var omitEmpty = context.MapperOptions.OmitEmptyStrings.ToString().ToLowerInvariant();
                var omitNull = context.MapperOptions.OmitNullStrings.ToString().ToLowerInvariant();

                sb.Append($".{setMethod}{genericArg}(\"{prop.DynamoKey}\", {propValue}{typeArgs}, {omitEmpty}, {omitNull})");
            }
        }

        return sb.ToString();
    }

    /// <summary>
    ///     Renders the FromItem code for a nested object (init-style assignment).
    /// </summary>
    private static string RenderNestedObjectFromInitAssignment(
        PropertyMappingSpec spec,
        PropertyAnalysis analysis,
        GeneratorContext context
    )
    {
        var nestedMapping = spec.TypeStrategy!.NestedMapping!;
        var itemParam = context.MapperOptions.FromMethodParameterName;

        return nestedMapping switch
        {
            MapperBasedNesting mapperBased => RenderMapperBasedFromInitAssignment(
                spec, itemParam, mapperBased.Mapper
            ),
            InlineNesting inline => RenderInlineFromInitAssignment(
                spec, itemParam, inline.Info, context
            ),
            _ => throw new InvalidOperationException($"Unknown nested mapping type: {nestedMapping.GetType()}")
        };
    }

    /// <summary>
    ///     Renders FromItem init-style code for mapper-based nested objects.
    ///     Output: PropertyName = item.TryGetValue("key", out var attr) &amp;&amp; attr.M is { } map ? MapperName.FromItem(map) : null,
    /// </summary>
    private static string RenderMapperBasedFromInitAssignment(
        PropertyMappingSpec spec,
        string itemParam,
        MapperReference mapper
    )
    {
        return $"{spec.PropertyName} = {itemParam}.TryGetValue(\"{spec.Key}\", out var {spec.PropertyName.ToLowerInvariant()}Attr) && {spec.PropertyName.ToLowerInvariant()}Attr.M is {{ }} {spec.PropertyName.ToLowerInvariant()}Map ? {mapper.MapperFullyQualifiedName}.FromItem({spec.PropertyName.ToLowerInvariant()}Map) : null,";
    }

    /// <summary>
    ///     Renders FromItem init-style code for inline nested objects.
    /// </summary>
    private static string RenderInlineFromInitAssignment(
        PropertyMappingSpec spec,
        string itemParam,
        NestedInlineInfo inlineInfo,
        GeneratorContext context
    )
    {
        var varName = spec.PropertyName.ToLowerInvariant();
        var initCode = RenderInlineNestedFromItem(varName + "Map", inlineInfo, context);

        return $"{spec.PropertyName} = {itemParam}.TryGetValue(\"{spec.Key}\", out var {varName}Attr) && {varName}Attr.M is {{ }} {varName}Map ? {initCode} : null,";
    }

    /// <summary>
    ///     Renders the inline object initializer for nested objects.
    /// </summary>
    private static string RenderInlineNestedFromItem(
        string mapVarName,
        NestedInlineInfo inlineInfo,
        GeneratorContext context
    )
    {
        var sb = new StringBuilder();
        sb.Append($"new {inlineInfo.ModelFullyQualifiedType} {{ ");

        var first = true;
        foreach (var prop in inlineInfo.Properties)
        {
            if (!first) sb.Append(" ");
            first = false;

            if (prop.NestedMapping is not null)
            {
                // Recursive nested object
                var nestedVarName = $"{mapVarName}_{prop.PropertyName.ToLowerInvariant()}";
                var nestedCode = prop.NestedMapping switch
                {
                    MapperBasedNesting mapperBased =>
                        $"{mapVarName}.TryGetValue(\"{prop.DynamoKey}\", out var {nestedVarName}Attr) && {nestedVarName}Attr.M is {{ }} {nestedVarName} ? {mapperBased.Mapper.MapperFullyQualifiedName}.FromItem({nestedVarName}) : null",
                    InlineNesting inline =>
                        $"{mapVarName}.TryGetValue(\"{prop.DynamoKey}\", out var {nestedVarName}Attr) && {nestedVarName}Attr.M is {{ }} {nestedVarName} ? {RenderInlineNestedFromItem(nestedVarName, inline.Info, context)} : null",
                    _ => throw new InvalidOperationException($"Unknown nested mapping type")
                };
                sb.Append($"{prop.PropertyName} = {nestedCode},");
            }
            else if (prop.Strategy is not null)
            {
                // Scalar property
                var getMethod = $"Get{prop.Strategy.NullableModifier}{prop.Strategy.TypeName}";
                var genericArg = prop.Strategy.GenericArgument;

                // Build type-specific args
                var typeArgs = prop.Strategy.FromTypeSpecificArgs.Length > 0
                    ? string.Join(", ", prop.Strategy.FromTypeSpecificArgs.Select(a =>
                        a.StartsWith("\"") ? $"format: {a}" : a)) + ", "
                    : "";

                sb.Append($"{prop.PropertyName} = {mapVarName}.{getMethod}{genericArg}(\"{prop.DynamoKey}\", {typeArgs}Requiredness.Optional),");
            }
        }

        sb.Append(" }");
        return sb.ToString();
    }

    /// <summary>
    ///     Renders the FromItem code for a nested object (regular assignment style).
    /// </summary>
    private static string RenderNestedObjectFromAssignment(
        PropertyMappingSpec spec,
        string modelVarName,
        PropertyAnalysis analysis,
        GeneratorContext context
    )
    {
        var nestedMapping = spec.TypeStrategy!.NestedMapping!;
        var itemParam = context.MapperOptions.FromMethodParameterName;

        return nestedMapping switch
        {
            MapperBasedNesting mapperBased => RenderMapperBasedFromAssignment(
                spec, modelVarName, itemParam, mapperBased.Mapper
            ),
            InlineNesting inline => RenderInlineFromAssignment(
                spec, modelVarName, itemParam, inline.Info, context
            ),
            _ => throw new InvalidOperationException($"Unknown nested mapping type: {nestedMapping.GetType()}")
        };
    }

    /// <summary>
    ///     Renders FromItem regular assignment code for mapper-based nested objects.
    /// </summary>
    private static string RenderMapperBasedFromAssignment(
        PropertyMappingSpec spec,
        string modelVarName,
        string itemParam,
        MapperReference mapper
    )
    {
        var varName = spec.PropertyName.ToLowerInvariant();
        return $"if ({itemParam}.TryGetValue(\"{spec.Key}\", out var {varName}Attr) && {varName}Attr.M is {{ }} {varName}Map) {modelVarName}.{spec.PropertyName} = {mapper.MapperFullyQualifiedName}.FromItem({varName}Map);";
    }

    /// <summary>
    ///     Renders FromItem regular assignment code for inline nested objects.
    /// </summary>
    private static string RenderInlineFromAssignment(
        PropertyMappingSpec spec,
        string modelVarName,
        string itemParam,
        NestedInlineInfo inlineInfo,
        GeneratorContext context
    )
    {
        var varName = spec.PropertyName.ToLowerInvariant();
        var initCode = RenderInlineNestedFromItem(varName + "Map", inlineInfo, context);
        return $"if ({itemParam}.TryGetValue(\"{spec.Key}\", out var {varName}Attr) && {varName}Attr.M is {{ }} {varName}Map) {modelVarName}.{spec.PropertyName} = {initCode};";
    }

    #endregion

    #region Nested Collection Rendering

    /// <summary>
    ///     Renders a nested collection property (List&lt;NestedType&gt; or Dictionary&lt;string, NestedType&gt;) into PropertyInfo.
    /// </summary>
    private static PropertyInfo RenderNestedCollectionProperty(
        PropertyMappingSpec spec,
        PropertyAnalysis analysis,
        string modelVarName,
        InitializationMethod initMethod,
        GeneratorContext context
    )
    {
        // Use init syntax for InitSyntax mode, regular assignment for PostConstruction
        var useInitSyntax = initMethod == InitializationMethod.InitSyntax;

        // FromItem for nested collections
        var fromAssignment =
            context.HasFromItemMethod
            && analysis.HasSetter
            && spec.FromItemMethod is not null
            && !useInitSyntax
                ? RenderNestedCollectionFromAssignment(spec, modelVarName, analysis, context)
                : null;

        var fromInitAssignment =
            context.HasFromItemMethod
            && analysis.HasSetter
            && spec.FromItemMethod is not null
            && useInitSyntax
                ? RenderNestedCollectionFromInitAssignment(spec, analysis, context)
                : null;

        // ToItem for nested collections
        var toAssignments =
            context.HasToItemMethod && analysis.HasGetter && spec.ToItemMethod is not null
                ? RenderNestedCollectionToAssignment(spec, analysis, context)
                : null;

        return new PropertyInfo(fromAssignment, fromInitAssignment, toAssignments, null);
    }

    /// <summary>
    ///     Renders the ToItem code for a nested collection.
    /// </summary>
    private static string RenderNestedCollectionToAssignment(
        PropertyMappingSpec spec,
        PropertyAnalysis analysis,
        GeneratorContext context
    )
    {
        var collectionInfo = spec.TypeStrategy!.CollectionInfo!;
        var elementMapping = collectionInfo.ElementNestedMapping!;
        var paramName = context.MapperOptions.ToMethodParameterName;
        var propAccess = $"{paramName}.{spec.PropertyName}";
        var isNullable = spec.TypeStrategy.NullableModifier == "Nullable";

        return collectionInfo.Category switch
        {
            CollectionCategory.List => RenderNestedListToAssignment(
                spec, propAccess, isNullable, elementMapping, collectionInfo, context
            ),
            CollectionCategory.Map => RenderNestedMapToAssignment(
                spec, propAccess, isNullable, elementMapping, context
            ),
            _ => throw new InvalidOperationException($"Unexpected collection category for nested collection: {collectionInfo.Category}")
        };
    }

    /// <summary>
    ///     Renders ToItem code for List&lt;NestedType&gt;.
    ///     Output: .Set("key", source.Prop?.Select(x => new AttributeValue { M = ... }).ToList())
    /// </summary>
    private static string RenderNestedListToAssignment(
        PropertyMappingSpec spec,
        string propAccess,
        bool isNullable,
        NestedMappingInfo elementMapping,
        CollectionInfo collectionInfo,
        GeneratorContext context
    )
    {
        var elementConverter = elementMapping switch
        {
            MapperBasedNesting mapperBased =>
                $"new AttributeValue {{ M = {mapperBased.Mapper.MapperFullyQualifiedName}.ToItem(x) }}",
            InlineNesting inline =>
                $"new AttributeValue {{ M = {RenderInlineNestedToItem("x", inline.Info, context)} }}",
            _ => throw new InvalidOperationException($"Unknown nested mapping type")
        };

        var selectExpr = $"{propAccess}{(isNullable ? "?" : "")}.Select(x => {elementConverter}).ToList()";

        if (isNullable)
        {
            return $".Set(\"{spec.Key}\", {propAccess} is null ? new AttributeValue {{ NULL = true }} : new AttributeValue {{ L = {selectExpr} }})";
        }

        return $".Set(\"{spec.Key}\", new AttributeValue {{ L = {selectExpr} }})";
    }

    /// <summary>
    ///     Renders ToItem code for Dictionary&lt;string, NestedType&gt;.
    ///     Output: .Set("key", source.Prop?.ToDictionary(kvp => kvp.Key, kvp => new AttributeValue { M = ... }))
    /// </summary>
    private static string RenderNestedMapToAssignment(
        PropertyMappingSpec spec,
        string propAccess,
        bool isNullable,
        NestedMappingInfo elementMapping,
        GeneratorContext context
    )
    {
        var valueConverter = elementMapping switch
        {
            MapperBasedNesting mapperBased =>
                $"new AttributeValue {{ M = {mapperBased.Mapper.MapperFullyQualifiedName}.ToItem(kvp.Value) }}",
            InlineNesting inline =>
                $"new AttributeValue {{ M = {RenderInlineNestedToItem("kvp.Value", inline.Info, context)} }}",
            _ => throw new InvalidOperationException($"Unknown nested mapping type")
        };

        var toDictExpr = $"{propAccess}{(isNullable ? "?" : "")}.ToDictionary(kvp => kvp.Key, kvp => {valueConverter})";

        if (isNullable)
        {
            return $".Set(\"{spec.Key}\", {propAccess} is null ? new AttributeValue {{ NULL = true }} : new AttributeValue {{ M = {toDictExpr} }})";
        }

        return $".Set(\"{spec.Key}\", new AttributeValue {{ M = {toDictExpr} }})";
    }

    /// <summary>
    ///     Renders the FromItem code for a nested collection (init-style assignment).
    /// </summary>
    private static string RenderNestedCollectionFromInitAssignment(
        PropertyMappingSpec spec,
        PropertyAnalysis analysis,
        GeneratorContext context
    )
    {
        var collectionInfo = spec.TypeStrategy!.CollectionInfo!;
        var elementMapping = collectionInfo.ElementNestedMapping!;
        var itemParam = context.MapperOptions.FromMethodParameterName;
        var varName = spec.PropertyName.ToLowerInvariant();

        return collectionInfo.Category switch
        {
            CollectionCategory.List => RenderNestedListFromInitAssignment(
                spec, itemParam, varName, elementMapping, collectionInfo, context
            ),
            CollectionCategory.Map => RenderNestedMapFromInitAssignment(
                spec, itemParam, varName, elementMapping, context
            ),
            _ => throw new InvalidOperationException($"Unexpected collection category for nested collection: {collectionInfo.Category}")
        };
    }

    /// <summary>
    ///     Renders FromItem init-style code for List&lt;NestedType&gt;.
    /// </summary>
    private static string RenderNestedListFromInitAssignment(
        PropertyMappingSpec spec,
        string itemParam,
        string varName,
        NestedMappingInfo elementMapping,
        CollectionInfo collectionInfo,
        GeneratorContext context
    )
    {
        var elementConverter = elementMapping switch
        {
            MapperBasedNesting mapperBased =>
                $"{mapperBased.Mapper.MapperFullyQualifiedName}.FromItem(av.M)",
            InlineNesting inline =>
                RenderInlineNestedFromItem("av.M", inline.Info, context),
            _ => throw new InvalidOperationException($"Unknown nested mapping type")
        };

        var selectExpr = $"{varName}List.Select(av => {elementConverter}).ToList()";

        // For arrays, add .ToArray()
        var toArray = collectionInfo.IsArray ? ".ToArray()" : "";

        return $"{spec.PropertyName} = {itemParam}.TryGetValue(\"{spec.Key}\", out var {varName}Attr) && {varName}Attr.L is {{ }} {varName}List ? {selectExpr}{toArray} : null,";
    }

    /// <summary>
    ///     Renders FromItem init-style code for Dictionary&lt;string, NestedType&gt;.
    /// </summary>
    private static string RenderNestedMapFromInitAssignment(
        PropertyMappingSpec spec,
        string itemParam,
        string varName,
        NestedMappingInfo elementMapping,
        GeneratorContext context
    )
    {
        var valueConverter = elementMapping switch
        {
            MapperBasedNesting mapperBased =>
                $"{mapperBased.Mapper.MapperFullyQualifiedName}.FromItem(kvp.Value.M)",
            InlineNesting inline =>
                RenderInlineNestedFromItem("kvp.Value.M", inline.Info, context),
            _ => throw new InvalidOperationException($"Unknown nested mapping type")
        };

        var toDictExpr = $"{varName}Map.ToDictionary(kvp => kvp.Key, kvp => {valueConverter})";

        return $"{spec.PropertyName} = {itemParam}.TryGetValue(\"{spec.Key}\", out var {varName}Attr) && {varName}Attr.M is {{ }} {varName}Map ? {toDictExpr} : null,";
    }

    /// <summary>
    ///     Renders the FromItem code for a nested collection (regular assignment style).
    /// </summary>
    private static string RenderNestedCollectionFromAssignment(
        PropertyMappingSpec spec,
        string modelVarName,
        PropertyAnalysis analysis,
        GeneratorContext context
    )
    {
        var collectionInfo = spec.TypeStrategy!.CollectionInfo!;
        var elementMapping = collectionInfo.ElementNestedMapping!;
        var itemParam = context.MapperOptions.FromMethodParameterName;
        var varName = spec.PropertyName.ToLowerInvariant();

        return collectionInfo.Category switch
        {
            CollectionCategory.List => RenderNestedListFromAssignment(
                spec, modelVarName, itemParam, varName, elementMapping, collectionInfo, context
            ),
            CollectionCategory.Map => RenderNestedMapFromAssignment(
                spec, modelVarName, itemParam, varName, elementMapping, context
            ),
            _ => throw new InvalidOperationException($"Unexpected collection category for nested collection: {collectionInfo.Category}")
        };
    }

    /// <summary>
    ///     Renders FromItem regular assignment code for List&lt;NestedType&gt;.
    /// </summary>
    private static string RenderNestedListFromAssignment(
        PropertyMappingSpec spec,
        string modelVarName,
        string itemParam,
        string varName,
        NestedMappingInfo elementMapping,
        CollectionInfo collectionInfo,
        GeneratorContext context
    )
    {
        var elementConverter = elementMapping switch
        {
            MapperBasedNesting mapperBased =>
                $"{mapperBased.Mapper.MapperFullyQualifiedName}.FromItem(av.M)",
            InlineNesting inline =>
                RenderInlineNestedFromItem("av.M", inline.Info, context),
            _ => throw new InvalidOperationException($"Unknown nested mapping type")
        };

        var selectExpr = $"{varName}List.Select(av => {elementConverter}).ToList()";

        // For arrays, add .ToArray()
        var toArray = collectionInfo.IsArray ? ".ToArray()" : "";

        return $"if ({itemParam}.TryGetValue(\"{spec.Key}\", out var {varName}Attr) && {varName}Attr.L is {{ }} {varName}List) {modelVarName}.{spec.PropertyName} = {selectExpr}{toArray};";
    }

    /// <summary>
    ///     Renders FromItem regular assignment code for Dictionary&lt;string, NestedType&gt;.
    /// </summary>
    private static string RenderNestedMapFromAssignment(
        PropertyMappingSpec spec,
        string modelVarName,
        string itemParam,
        string varName,
        NestedMappingInfo elementMapping,
        GeneratorContext context
    )
    {
        var valueConverter = elementMapping switch
        {
            MapperBasedNesting mapperBased =>
                $"{mapperBased.Mapper.MapperFullyQualifiedName}.FromItem(kvp.Value.M)",
            InlineNesting inline =>
                RenderInlineNestedFromItem("kvp.Value.M", inline.Info, context),
            _ => throw new InvalidOperationException($"Unknown nested mapping type")
        };

        var toDictExpr = $"{varName}Map.ToDictionary(kvp => kvp.Key, kvp => {valueConverter})";

        return $"if ({itemParam}.TryGetValue(\"{spec.Key}\", out var {varName}Attr) && {varName}Attr.M is {{ }} {varName}Map) {modelVarName}.{spec.PropertyName} = {toDictExpr};";
    }

    #endregion
}
