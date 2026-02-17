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
        PropertyMappingSpec spec, PropertyAnalysis analysis, string modelVarName, int index,
        InitializationMethod initMethod, GeneratorContext context,
        HelperMethodRegistry helperRegistry
    )
    {
        // Check if this is a nested object - requires special handling
        var isNestedObject =
            spec.TypeStrategy?.NestedMapping is not null &&
            spec.TypeStrategy.CollectionInfo is null;

        // Check if this is a nested collection (List<NestedType> or Dictionary<string, NestedType>)
        var isNestedCollection =
            spec.TypeStrategy?.CollectionInfo?.ElementNestedMapping is not null;

        // If this property is a constructor parameter, render as constructor argument
        if (initMethod == InitializationMethod.ConstructorParameter && context.HasFromItemMethod)
        {
            // Nested objects/collections in constructors are not yet supported
            var constructorArg =
                spec.FromItemMethod is not null && !isNestedObject && !isNestedCollection
                    ? RenderConstructorArgument(spec, analysis, context)
                    : null;

            // Constructor parameters still need ToAssignments for serialization
            string? toAssignment = null;
            if (context.HasToItemMethod && analysis.HasGetter && spec.ToItemMethod is not null)
            {
                if (isNestedObject)
                    toAssignment =
                        RenderNestedObjectToAssignment(spec, analysis, context, helperRegistry);
                else if (isNestedCollection)
                    toAssignment =
                        RenderNestedCollectionToAssignment(spec, analysis, context, helperRegistry);
                else
                    toAssignment = RenderToAssignment(spec);
            }

            return new PropertyInfo(null, null, toAssignment, constructorArg);
        }

        // Handle nested collections specially (List<Address>, Dictionary<string, Address>)
        if (isNestedCollection)
        {
            return RenderNestedCollectionProperty(
                spec,
                analysis,
                modelVarName,
                initMethod,
                context,
                helperRegistry
            );
        }

        // Handle nested objects specially
        if (isNestedObject)
        {
            return RenderNestedObjectProperty(
                spec,
                analysis,
                modelVarName,
                initMethod,
                context,
                helperRegistry
            );
        }

        // Determine if we should use regular assignment vs init assignment
        var useRegularAssignment =
            spec.FromItemMethod is { IsCustomMethod: false } && analysis is
                { IsRequired: false, IsInitOnly: false, HasDefaultValue: true };

        // Use init syntax for InitSyntax mode, regular assignment for PostConstruction
        var useInitSyntax = initMethod == InitializationMethod.InitSyntax;

        // FromItem requires both: setter on property AND FromItem method exists
        var fromAssignment =
            context.HasFromItemMethod && analysis.HasSetter && spec.FromItemMethod is not null &&
            (!useInitSyntax || useRegularAssignment)
                ? RenderFromAssignment(spec, modelVarName, analysis, index, context)
                : null;

        var fromInitAssignment =
            context.HasFromItemMethod && analysis.HasSetter && spec.FromItemMethod is not null &&
            useInitSyntax && !useRegularAssignment
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
        PropertyMappingSpec spec, PropertyAnalysis analysis, string modelVarName,
        InitializationMethod initMethod, GeneratorContext context,
        HelperMethodRegistry helperRegistry
    )
    {
        // Use init syntax for InitSyntax mode, regular assignment for PostConstruction
        var useInitSyntax = initMethod == InitializationMethod.InitSyntax;

        // FromItem for nested objects
        var fromAssignment =
            context.HasFromItemMethod && analysis.HasSetter && spec.FromItemMethod is not null &&
            !useInitSyntax
                ? RenderNestedObjectFromAssignment(
                    spec,
                    modelVarName,
                    analysis,
                    context,
                    helperRegistry
                )
                : null;

        var fromInitAssignment =
            context.HasFromItemMethod && analysis.HasSetter && spec.FromItemMethod is not null &&
            useInitSyntax
                ? RenderNestedObjectFromInitAssignment(spec, analysis, context, helperRegistry)
                : null;

        // ToItem for nested objects
        var toAssignments =
            context.HasToItemMethod && analysis.HasGetter && spec.ToItemMethod is not null
                ? RenderNestedObjectToAssignment(spec, analysis, context, helperRegistry)
                : null;

        return new PropertyInfo(fromAssignment, fromInitAssignment, toAssignments, null);
    }

    /// <summary>
    ///     Renders the FromAssignment string for deserialization. Format: PropertyName =
    ///     paramName.MethodName&lt;Generic&gt;(args), OR PropertyName = CustomMethodName(args), for
    ///     custom methods
    /// </summary>
    private static string RenderFromInitAssignment(
        PropertyMappingSpec spec, PropertyAnalysis analysis, GeneratorContext context
    )
    {
        Debug.Assert(spec.FromItemMethod is not null, "FromItemMethod should not be null");
        Debug.Assert(
            spec.FromItemMethod!.IsCustomMethod || spec.TypeStrategy is not null,
            "TypeStrategy should not be null for standard methods"
        );

        var args = string.Join(", ", spec.FromItemMethod.Arguments.Select(a => a.Value));

        var methodCall =
            spec.FromItemMethod.IsCustomMethod
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
        PropertyMappingSpec spec, string modelVarName, PropertyAnalysis analysis, int index,
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
            spec.TypeStrategy?.TypeName == "Enum" &&
            spec.TypeStrategy?.NullableModifier == "Nullable" &&
            argsList.Any(a => a.Contains("format:"));
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

        return
            $"if ({context.MapperOptions.FromMethodParameterName}.{methodCall}) {modelVarName}.{spec.PropertyName} = var{index}!{toArray};";
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

        var methodCall =
            spec.ToItemMethod.IsCustomMethod
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
        PropertyMappingSpec spec, PropertyAnalysis analysis, GeneratorContext context
    )
    {
        Debug.Assert(spec.FromItemMethod is not null, "FromItemMethod should not be null");
        Debug.Assert(
            spec.FromItemMethod!.IsCustomMethod || spec.TypeStrategy is not null,
            "TypeStrategy should not be null for standard methods"
        );

        var args = string.Join(", ", spec.FromItemMethod.Arguments.Select(a => a.Value));

        var methodCall =
            spec.FromItemMethod.IsCustomMethod
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
        PropertyMappingSpec spec, PropertyAnalysis analysis, GeneratorContext context,
        HelperMethodRegistry helperRegistry
    )
    {
        var nestedMapping = spec.TypeStrategy!.NestedMapping!;
        var paramName = context.MapperOptions.ToMethodParameterName;
        var isNullable = spec.TypeStrategy.NullableModifier == "Nullable";

        return nestedMapping switch
        {
            MapperBasedNesting mapperBased => RenderMapperBasedToAssignment(
                spec,
                paramName,
                isNullable,
                mapperBased.Mapper
            ),
            InlineNesting inline => RenderInlineToAssignment(
                spec,
                paramName,
                isNullable,
                inline.Info,
                context,
                helperRegistry
            ),
            _ => throw new InvalidOperationException(
                $"Unknown nested mapping type: {nestedMapping.GetType()}"
            ),
        };
    }

    /// <summary>
    ///     Renders ToItem code for mapper-based nested objects.
    ///     Output: .Set("key", source.Prop is null ? new AttributeValue { NULL = true } : new AttributeValue { M = MapperName.ToItem(source.Prop) })
    /// </summary>
    private static string RenderMapperBasedToAssignment(
        PropertyMappingSpec spec, string paramName, bool isNullable, MapperReference mapper
    )
    {
        var propAccess = $"{paramName}.{spec.PropertyName}";

        if (isNullable)
        {
            return
                $".Set(\"{spec.Key}\", {propAccess} is null ? new AttributeValue {{ NULL = true }} : new AttributeValue {{ M = {mapper.MapperFullyQualifiedName}.ToItem({propAccess}) }})";
        }

        return
            $".Set(\"{spec.Key}\", new AttributeValue {{ M = {mapper.MapperFullyQualifiedName}.ToItem({propAccess}) }})";
    }

    /// <summary>
    ///     Renders ToItem code for inline nested objects using helper methods.
    ///     Output: .Set("key", source.Prop is null ? new AttributeValue { NULL = true } : new AttributeValue { M = ToItem_Type(source.Prop) })
    /// </summary>
    private static string RenderInlineToAssignment(
        PropertyMappingSpec spec, string paramName, bool isNullable, NestedInlineInfo inlineInfo,
        GeneratorContext context, HelperMethodRegistry helperRegistry
    )
    {
        var propAccess = $"{paramName}.{spec.PropertyName}";

        // Register the helper method and get its name
        var helperMethodName =
            helperRegistry.GetOrRegisterToItemHelper(
                inlineInfo.ModelFullyQualifiedType,
                inlineInfo
            );

        if (isNullable)
        {
            return
                $".Set(\"{spec.Key}\", {propAccess} is null ? new AttributeValue {{ NULL = true }} : new AttributeValue {{ M = {helperMethodName}({propAccess}) }})";
        }

        return
            $".Set(\"{spec.Key}\", new AttributeValue {{ M = {helperMethodName}({propAccess}) }})";
    }

    /// <summary>
    ///     Renders the inline Dictionary creation code for nested objects.
    ///     Made internal to allow reuse by HelperMethodEmitter.
    /// </summary>
    internal static string RenderInlineNestedToItem(
        string sourcePrefix, NestedInlineInfo inlineInfo, GeneratorContext context,
        HelperMethodRegistry helperRegistry
    )
    {
        var sb = new StringBuilder();
        sb.Append("new Dictionary<string, AttributeValue>()");

        foreach (var prop in inlineInfo.Properties)
        {
            var call = RenderToItemPropertyCall(prop, sourcePrefix, context, helperRegistry);
            if (call is not null)
                sb.Append(call);
        }

        return sb.ToString();
    }

    /// <summary>
    ///     Renders a single property's .Set*(...) call fragment for ToItem mapping. Returns null if
    ///     the property produces no call (no getter, no strategy, no nested mapping). Made internal to
    ///     allow reuse by HelperMethodEmitter for formatted multi-line rendering.
    /// </summary>
    internal static string? RenderToItemPropertyCall(
        NestedPropertySpec prop, string sourcePrefix, GeneratorContext context,
        HelperMethodRegistry helperRegistry
    )
    {
        if (prop.NestedMapping is not null)
        {
            var nestedSourcePrefix = $"{sourcePrefix}.{prop.PropertyName}";

            string nestedCode;
            if (prop.NestedMapping is MapperBasedNesting mapperBased)
            {
                nestedCode =
                    $"new AttributeValue {{ M = {mapperBased.Mapper.MapperFullyQualifiedName}.ToItem({nestedSourcePrefix}) }}";
            }
            else if (prop.NestedMapping is InlineNesting inline)
            {
                var helperMethodName =
                    helperRegistry.GetOrRegisterToItemHelper(
                        inline.Info.ModelFullyQualifiedType,
                        inline.Info
                    );
                nestedCode =
                    $"new AttributeValue {{ M = {helperMethodName}({nestedSourcePrefix}) }}";
            }
            else
            {
                throw new InvalidOperationException("Unknown nested mapping type");
            }

            return
                $".Set(\"{prop.DynamoKey}\", {nestedSourcePrefix} is null ? new AttributeValue {{ NULL = true }} : {nestedCode})";
        }

        if (prop.Strategy is not null && prop.HasGetter)
        {
            var setMethod = $"Set{prop.Strategy.TypeName}";
            var genericArg = prop.Strategy.GenericArgument;
            var propValue = $"{sourcePrefix}.{prop.PropertyName}";

            var typeArgs =
                prop.Strategy.ToTypeSpecificArgs.Length > 0
                    ? ", " + string.Join(", ", prop.Strategy.ToTypeSpecificArgs)
                    : "";

            var omitEmpty = context.MapperOptions.OmitEmptyStrings.ToString().ToLowerInvariant();
            var omitNull = context.MapperOptions.OmitNullStrings.ToString().ToLowerInvariant();

            return
                $".{setMethod}{genericArg}(\"{prop.DynamoKey}\", {propValue}{typeArgs}, {omitEmpty}, {omitNull})";
        }

        return null;
    }

    /// <summary>
    ///     Renders the FromItem code for a nested object (init-style assignment).
    /// </summary>
    private static string RenderNestedObjectFromInitAssignment(
        PropertyMappingSpec spec, PropertyAnalysis analysis, GeneratorContext context,
        HelperMethodRegistry helperRegistry
    )
    {
        var nestedMapping = spec.TypeStrategy!.NestedMapping!;
        var itemParam = context.MapperOptions.FromMethodParameterName;

        // Determine fallback based on required keyword and nullability
        var fallback = GetNestedObjectFallback(spec, analysis);

        return nestedMapping switch
        {
            MapperBasedNesting mapperBased => RenderMapperBasedFromInitAssignment(
                spec,
                itemParam,
                mapperBased.Mapper,
                fallback
            ),
            InlineNesting inline => RenderInlineFromInitAssignment(
                spec,
                itemParam,
                inline.Info,
                context,
                fallback,
                helperRegistry
            ),
            _ => throw new InvalidOperationException(
                $"Unknown nested mapping type: {nestedMapping.GetType()}"
            ),
        };
    }

    /// <summary>
    ///     Determines the fallback expression for a nested object when not found in DynamoDB.
    /// </summary>
    private static string GetNestedObjectFallback(
        PropertyMappingSpec spec, PropertyAnalysis analysis
    )
    {
        // If property has 'required' keyword, throw on missing
        if (analysis.IsRequired)
            return
                $"throw new System.InvalidOperationException(\"Required attribute '{spec.Key}' not found.\")";

        // For nullable types, return null
        if (analysis.Nullability.IsNullableType)
            return "null";

        // For non-nullable, non-required types, we still use null but this may cause CS8601
        // This is a design limitation - the model should either mark as required or nullable
        return "null";
    }

    /// <summary>
    ///     Renders FromItem init-style code for mapper-based nested objects.
    ///     Output: PropertyName = item.TryGetValue("key", out var attr) &amp;&amp; attr.M is { } map ? MapperName.FromItem(map) : fallback,
    /// </summary>
    private static string RenderMapperBasedFromInitAssignment(
        PropertyMappingSpec spec, string itemParam, MapperReference mapper, string fallback
    )
    {
        var varName = spec.PropertyName.ToLowerInvariant();
        return
            $"{spec.PropertyName} = {itemParam}.TryGetValue(\"{spec.Key}\", out var {varName}Attr) && {varName}Attr.M is {{ }} {varName}Map ? {mapper.MapperFullyQualifiedName}.FromItem({varName}Map) : {fallback},";
    }

    /// <summary>
    ///     Renders FromItem init-style code for inline nested objects.
    /// </summary>
    private static string RenderInlineFromInitAssignment(
        PropertyMappingSpec spec, string itemParam, NestedInlineInfo inlineInfo,
        GeneratorContext context, string fallback, HelperMethodRegistry helperRegistry
    )
    {
        var varName = spec.PropertyName.ToLowerInvariant();

        // Register the helper method and get its name
        var helperMethodName =
            helperRegistry.GetOrRegisterFromItemHelper(
                inlineInfo.ModelFullyQualifiedType,
                inlineInfo
            );

        return
            $"{spec.PropertyName} = {itemParam}.TryGetValue(\"{spec.Key}\", out var {varName}Attr) && {varName}Attr.M is {{ }} {varName}Map ? {helperMethodName}({varName}Map) : {fallback},";
    }

    /// <summary>
    ///     Renders the inline object initializer for nested objects.
    ///     Made internal to allow reuse by HelperMethodEmitter.
    /// </summary>
    internal static string RenderInlineNestedFromItem(
        string mapVarName, NestedInlineInfo inlineInfo, GeneratorContext context,
        HelperMethodRegistry helperRegistry
    )
    {
        // Split properties into object initializer vs post-construction
        var initProperties = new List<NestedPropertySpec>();
        var postConstructionProperties = new List<NestedPropertySpec>();

        foreach (var prop in inlineInfo.Properties)
        {
            // Use post-construction for optional, settable properties with default values
            var usePostConstruction =
                !prop.IsRequired && !prop.IsInitOnly && prop.HasDefaultValue && prop.HasSetter;

            if (usePostConstruction)
                postConstructionProperties.Add(prop);
            else
                initProperties.Add(prop);
        }

        // If no post-construction properties, use simple expression
        if (postConstructionProperties.Count == 0)
            return RenderSimpleNestedFromItem(
                mapVarName,
                inlineInfo.ModelFullyQualifiedType,
                initProperties,
                context,
                helperRegistry
            );

        // Otherwise, use var + initializer + if statements pattern
        var sb = new StringBuilder();
        var varName = TypeNameHelper.ToVariableName(inlineInfo.ModelFullyQualifiedType);

        // Variable declaration with object initializer for required/init properties
        if (initProperties.Count == 0)
        {
            sb.AppendLine($"    var {varName} = new {inlineInfo.ModelFullyQualifiedType}();");
        }
        else
        {
            sb.AppendLine($"    var {varName} = new {inlineInfo.ModelFullyQualifiedType}");
            sb.AppendLine("    {");
            foreach (var prop in initProperties)
            {
                sb.Append("        ");
                RenderPropertyInitAssignment(prop, mapVarName, sb, context, helperRegistry);
                sb.AppendLine();
            }

            sb.AppendLine("    };");
        }

        // Post-construction if statements for optional properties with defaults
        var varIndex = 0;
        foreach (var prop in postConstructionProperties)
        {
            sb.Append("    if (");
            sb.Append(RenderTryGetCondition(prop, mapVarName, $"var{varIndex}", context));
            sb.Append($") {varName}.{prop.PropertyName} = var{varIndex}!;");
            sb.AppendLine();
            varIndex++;
        }

        sb.Append($"    return {varName};");
        return sb.ToString();
    }

    /// <summary>Renders a simple single-expression nested object creation (all properties in initializer).</summary>
    private static string RenderSimpleNestedFromItem(
        string mapVarName, string modelType, List<NestedPropertySpec> properties,
        GeneratorContext context, HelperMethodRegistry helperRegistry
    )
    {
        var sb = new StringBuilder();
        sb.Append($"new {modelType} {{ ");

        var first = true;
        foreach (var prop in properties)
        {
            if (!first) sb.Append(" ");
            first = false;

            RenderPropertyInitAssignment(prop, mapVarName, sb, context, helperRegistry);
        }

        sb.Append(" }");
        return sb.ToString();
    }

    /// <summary>
    ///     Renders a single property initialization assignment for object initializer. Made internal
    ///     to allow reuse by HelperMethodEmitter for formatted multi-line rendering.
    /// </summary>
    internal static void RenderPropertyInitAssignment(
        NestedPropertySpec prop, string mapVarName, StringBuilder sb, GeneratorContext context,
        HelperMethodRegistry helperRegistry
    )
    {
        if (prop.NestedMapping is not null)
        {
            // Recursive nested object
            var nestedVarName = $"{mapVarName}_{prop.PropertyName.ToLowerInvariant()}";

            // Determine fallback based on required and nullability
            string fallback;
            if (prop.IsRequired)
                fallback =
                    $"throw new System.InvalidOperationException(\"Required nested property '{prop.DynamoKey}' not found in DynamoDB item.\")";
            else if (prop.Nullability.IsNullableType)
                fallback = "null";
            else
                // Non-nullable, non-required - use null (design limitation)
                fallback = "null";

            string nestedCode;
            if (prop.NestedMapping is MapperBasedNesting mapperBased)
            {
                nestedCode =
                    $"{mapVarName}.TryGetValue(\"{prop.DynamoKey}\", out var {nestedVarName}Attr) && {nestedVarName}Attr.M is {{ }} {nestedVarName} ? {mapperBased.Mapper.MapperFullyQualifiedName}.FromItem({nestedVarName}) : {fallback}";
            }
            else if (prop.NestedMapping is InlineNesting inline)
            {
                // Register helper and generate call instead of inlining
                var helperMethodName =
                    helperRegistry.GetOrRegisterFromItemHelper(
                        inline.Info.ModelFullyQualifiedType,
                        inline.Info
                    );
                nestedCode =
                    $"{mapVarName}.TryGetValue(\"{prop.DynamoKey}\", out var {nestedVarName}Attr) && {nestedVarName}Attr.M is {{ }} {nestedVarName} ? {helperMethodName}({nestedVarName}) : {fallback}";
            }
            else
            {
                throw new InvalidOperationException("Unknown nested mapping type");
            }

            sb.Append($"{prop.PropertyName} = {nestedCode},");
        }
        else if (prop.Strategy is not null)
        {
            // Scalar property
            var getMethod = $"Get{prop.Strategy.NullableModifier}{prop.Strategy.TypeName}";
            var genericArg = prop.Strategy.GenericArgument;

            // Build type-specific args
            var typeArgs =
                prop.Strategy.FromTypeSpecificArgs.Length > 0
                    ? string.Join(
                        ", ",
                        prop.Strategy.FromTypeSpecificArgs.Select(
                            a => a.StartsWith("\"") ? $"format: {a}" : a
                        )
                    ) + ", "
                    : "";

            // Determine requiredness based on property analysis
            var requiredness = prop.IsRequired ? "Requiredness.Required" : "Requiredness.Optional";

            sb.Append(
                $"{prop.PropertyName} = {mapVarName}.{getMethod}{genericArg}(\"{prop.DynamoKey}\", {typeArgs}{requiredness}),"
            );
        }
    }

    /// <summary>Renders a TryGet condition for post-construction assignment.</summary>
    private static string RenderTryGetCondition(
        NestedPropertySpec prop, string mapVarName, string outVarName, GeneratorContext context
    )
    {
        if (prop.Strategy is null)
            throw new InvalidOperationException(
                "Post-construction only supported for scalar properties"
            );

        var tryMethod = $"TryGet{prop.Strategy.NullableModifier}{prop.Strategy.TypeName}";
        var genericArg = prop.Strategy.GenericArgument;

        // Build type-specific args
        var typeArgs =
            prop.Strategy.FromTypeSpecificArgs.Length > 0
                ? string.Join(
                    ", ",
                    prop.Strategy.FromTypeSpecificArgs.Select(
                        a => a.StartsWith("\"") ? $"format: {a}" : a
                    )
                ) + ", "
                : "";

        return
            $"{mapVarName}.{tryMethod}{genericArg}(\"{prop.DynamoKey}\", out var {outVarName}, {typeArgs}Requiredness.InferFromNullability)";
    }

    /// <summary>
    ///     Renders the FromItem code for a nested object (regular assignment style).
    /// </summary>
    private static string RenderNestedObjectFromAssignment(
        PropertyMappingSpec spec, string modelVarName, PropertyAnalysis analysis,
        GeneratorContext context, HelperMethodRegistry helperRegistry
    )
    {
        var nestedMapping = spec.TypeStrategy!.NestedMapping!;
        var itemParam = context.MapperOptions.FromMethodParameterName;

        return nestedMapping switch
        {
            MapperBasedNesting mapperBased => RenderMapperBasedFromAssignment(
                spec,
                modelVarName,
                itemParam,
                mapperBased.Mapper
            ),
            InlineNesting inline => RenderInlineFromAssignment(
                spec,
                modelVarName,
                itemParam,
                inline.Info,
                context,
                helperRegistry
            ),
            _ => throw new InvalidOperationException(
                $"Unknown nested mapping type: {nestedMapping.GetType()}"
            ),
        };
    }

    /// <summary>
    ///     Renders FromItem regular assignment code for mapper-based nested objects.
    /// </summary>
    private static string RenderMapperBasedFromAssignment(
        PropertyMappingSpec spec, string modelVarName, string itemParam, MapperReference mapper
    )
    {
        var varName = spec.PropertyName.ToLowerInvariant();
        return
            $"if ({itemParam}.TryGetValue(\"{spec.Key}\", out var {varName}Attr) && {varName}Attr.M is {{ }} {varName}Map) {modelVarName}.{spec.PropertyName} = {mapper.MapperFullyQualifiedName}.FromItem({varName}Map);";
    }

    /// <summary>
    ///     Renders FromItem regular assignment code for inline nested objects.
    /// </summary>
    private static string RenderInlineFromAssignment(
        PropertyMappingSpec spec, string modelVarName, string itemParam,
        NestedInlineInfo inlineInfo, GeneratorContext context, HelperMethodRegistry helperRegistry
    )
    {
        var varName = spec.PropertyName.ToLowerInvariant();

        // Register the helper method and get its name
        var helperMethodName =
            helperRegistry.GetOrRegisterFromItemHelper(
                inlineInfo.ModelFullyQualifiedType,
                inlineInfo
            );

        return
            $"if ({itemParam}.TryGetValue(\"{spec.Key}\", out var {varName}Attr) && {varName}Attr.M is {{ }} {varName}Map) {modelVarName}.{spec.PropertyName} = {helperMethodName}({varName}Map);";
    }

    #endregion

    #region Nested Collection Rendering

    /// <summary>
    ///     Renders a nested collection property (List&lt;NestedType&gt; or Dictionary&lt;string, NestedType&gt;) into PropertyInfo.
    /// </summary>
    private static PropertyInfo RenderNestedCollectionProperty(
        PropertyMappingSpec spec, PropertyAnalysis analysis, string modelVarName,
        InitializationMethod initMethod, GeneratorContext context,
        HelperMethodRegistry helperRegistry
    )
    {
        // Use init syntax for InitSyntax mode, regular assignment for PostConstruction
        var useInitSyntax = initMethod == InitializationMethod.InitSyntax;

        // FromItem for nested collections
        var fromAssignment =
            context.HasFromItemMethod && analysis.HasSetter && spec.FromItemMethod is not null &&
            !useInitSyntax
                ? RenderNestedCollectionFromAssignment(
                    spec,
                    modelVarName,
                    analysis,
                    context,
                    helperRegistry
                )
                : null;

        var fromInitAssignment =
            context.HasFromItemMethod && analysis.HasSetter && spec.FromItemMethod is not null &&
            useInitSyntax
                ? RenderNestedCollectionFromInitAssignment(spec, analysis, context, helperRegistry)
                : null;

        // ToItem for nested collections
        var toAssignments =
            context.HasToItemMethod && analysis.HasGetter && spec.ToItemMethod is not null
                ? RenderNestedCollectionToAssignment(spec, analysis, context, helperRegistry)
                : null;

        return new PropertyInfo(fromAssignment, fromInitAssignment, toAssignments, null);
    }

    /// <summary>
    ///     Renders the ToItem code for a nested collection.
    /// </summary>
    private static string RenderNestedCollectionToAssignment(
        PropertyMappingSpec spec, PropertyAnalysis analysis, GeneratorContext context,
        HelperMethodRegistry helperRegistry
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
                spec,
                propAccess,
                isNullable,
                elementMapping,
                collectionInfo,
                context,
                helperRegistry
            ),
            CollectionCategory.Map => RenderNestedMapToAssignment(
                spec,
                propAccess,
                isNullable,
                elementMapping,
                context,
                helperRegistry
            ),
            _ => throw new InvalidOperationException(
                $"Unexpected collection category for nested collection: {collectionInfo.Category}"
            ),
        };
    }

    /// <summary>
    ///     Renders ToItem code for List&lt;NestedType&gt;.
    ///     Output: .Set("key", source.Prop?.Select(x => new AttributeValue { M = ... }).ToList())
    /// </summary>
    private static string RenderNestedListToAssignment(
        PropertyMappingSpec spec, string propAccess, bool isNullable,
        NestedMappingInfo elementMapping, CollectionInfo collectionInfo, GeneratorContext context,
        HelperMethodRegistry helperRegistry
    )
    {
        var elementConverter =
            elementMapping switch
            {
                MapperBasedNesting mapperBased =>
                    $"new AttributeValue {{ M = {mapperBased.Mapper.MapperFullyQualifiedName}.ToItem(x) }}",
                InlineNesting inline =>
                    $"new AttributeValue {{ M = {helperRegistry.GetOrRegisterToItemHelper(inline.Info.ModelFullyQualifiedType, inline.Info)}(x) }}",
                _ => throw new InvalidOperationException("Unknown nested mapping type"),
            };

        var selectExpr =
            $"{propAccess}{(isNullable ? "?" : "")}.Select(x => {elementConverter}).ToList()";

        if (isNullable)
        {
            return
                $".Set(\"{spec.Key}\", {propAccess} is null ? new AttributeValue {{ NULL = true }} : new AttributeValue {{ L = {selectExpr} }})";
        }

        return $".Set(\"{spec.Key}\", new AttributeValue {{ L = {selectExpr} }})";
    }

    /// <summary>
    ///     Renders ToItem code for Dictionary&lt;string, NestedType&gt;.
    ///     Output: .Set("key", source.Prop?.ToDictionary(kvp => kvp.Key, kvp => new AttributeValue { M = ... }))
    /// </summary>
    private static string RenderNestedMapToAssignment(
        PropertyMappingSpec spec, string propAccess, bool isNullable,
        NestedMappingInfo elementMapping, GeneratorContext context,
        HelperMethodRegistry helperRegistry
    )
    {
        var valueConverter =
            elementMapping switch
            {
                MapperBasedNesting mapperBased =>
                    $"new AttributeValue {{ M = {mapperBased.Mapper.MapperFullyQualifiedName}.ToItem(kvp.Value) }}",
                InlineNesting inline =>
                    $"new AttributeValue {{ M = {helperRegistry.GetOrRegisterToItemHelper(inline.Info.ModelFullyQualifiedType, inline.Info)}(kvp.Value) }}",
                _ => throw new InvalidOperationException("Unknown nested mapping type"),
            };

        var toDictExpr =
            $"{propAccess}{(isNullable ? "?" : "")}.ToDictionary(kvp => kvp.Key, kvp => {valueConverter})";

        if (isNullable)
        {
            return
                $".Set(\"{spec.Key}\", {propAccess} is null ? new AttributeValue {{ NULL = true }} : new AttributeValue {{ M = {toDictExpr} }})";
        }

        return $".Set(\"{spec.Key}\", new AttributeValue {{ M = {toDictExpr} }})";
    }

    /// <summary>
    ///     Renders the FromItem code for a nested collection (init-style assignment).
    /// </summary>
    private static string RenderNestedCollectionFromInitAssignment(
        PropertyMappingSpec spec, PropertyAnalysis analysis, GeneratorContext context,
        HelperMethodRegistry helperRegistry
    )
    {
        var collectionInfo = spec.TypeStrategy!.CollectionInfo!;
        var elementMapping = collectionInfo.ElementNestedMapping!;
        var itemParam = context.MapperOptions.FromMethodParameterName;
        var varName = spec.PropertyName.ToLowerInvariant();

        // Determine fallback based on required keyword and nullability
        var fallback = GetNestedCollectionFallback(spec, analysis);

        return collectionInfo.Category switch
        {
            CollectionCategory.List => RenderNestedListFromInitAssignment(
                spec,
                itemParam,
                varName,
                elementMapping,
                collectionInfo,
                context,
                fallback,
                helperRegistry
            ),
            CollectionCategory.Map => RenderNestedMapFromInitAssignment(
                spec,
                itemParam,
                varName,
                elementMapping,
                context,
                fallback,
                helperRegistry
            ),
            _ => throw new InvalidOperationException(
                $"Unexpected collection category for nested collection: {collectionInfo.Category}"
            ),
        };
    }

    /// <summary>
    ///     Determines the fallback expression for a nested collection when not found in DynamoDB.
    /// </summary>
    private static string GetNestedCollectionFallback(
        PropertyMappingSpec spec, PropertyAnalysis analysis
    )
    {
        // If property has 'required' keyword, throw on missing
        if (analysis.IsRequired)
            return
                $"throw new System.InvalidOperationException(\"Required attribute '{spec.Key}' not found.\")";

        // For nullable types, return null
        if (analysis.Nullability.IsNullableType)
            return "null";

        // For non-nullable, non-required collections, return empty collection
        // Using collection expression [] which works for arrays, lists, and dictionaries in C# 12+
        return "[]";
    }

    /// <summary>
    ///     Renders FromItem init-style code for List&lt;NestedType&gt;.
    /// </summary>
    private static string RenderNestedListFromInitAssignment(
        PropertyMappingSpec spec, string itemParam, string varName,
        NestedMappingInfo elementMapping, CollectionInfo collectionInfo, GeneratorContext context,
        string fallback, HelperMethodRegistry helperRegistry
    )
    {
        var elementConverter =
            elementMapping switch
            {
                MapperBasedNesting mapperBased =>
                    $"{mapperBased.Mapper.MapperFullyQualifiedName}.FromItem(av.M)",
                InlineNesting inline =>
                    $"{helperRegistry.GetOrRegisterFromItemHelper(inline.Info.ModelFullyQualifiedType, inline.Info)}(av.M)",
                _ => throw new InvalidOperationException("Unknown nested mapping type"),
            };

        var selectExpr = $"{varName}List.Select(av => {elementConverter}).ToList()";

        // For arrays, add .ToArray()
        var toArray = collectionInfo.IsArray ? ".ToArray()" : "";

        return
            $"{spec.PropertyName} = {itemParam}.TryGetValue(\"{spec.Key}\", out var {varName}Attr) && {varName}Attr.L is {{ }} {varName}List ? {selectExpr}{toArray} : {fallback},";
    }

    /// <summary>
    ///     Renders FromItem init-style code for Dictionary&lt;string, NestedType&gt;.
    /// </summary>
    private static string RenderNestedMapFromInitAssignment(
        PropertyMappingSpec spec, string itemParam, string varName,
        NestedMappingInfo elementMapping, GeneratorContext context, string fallback,
        HelperMethodRegistry helperRegistry
    )
    {
        var valueConverter =
            elementMapping switch
            {
                MapperBasedNesting mapperBased =>
                    $"{mapperBased.Mapper.MapperFullyQualifiedName}.FromItem(kvp.Value.M)",
                InlineNesting inline =>
                    $"{helperRegistry.GetOrRegisterFromItemHelper(inline.Info.ModelFullyQualifiedType, inline.Info)}(kvp.Value.M)",
                _ => throw new InvalidOperationException("Unknown nested mapping type"),
            };

        var toDictExpr = $"{varName}Map.ToDictionary(kvp => kvp.Key, kvp => {valueConverter})";

        return
            $"{spec.PropertyName} = {itemParam}.TryGetValue(\"{spec.Key}\", out var {varName}Attr) && {varName}Attr.M is {{ }} {varName}Map ? {toDictExpr} : {fallback},";
    }

    /// <summary>
    ///     Renders the FromItem code for a nested collection (regular assignment style).
    /// </summary>
    private static string RenderNestedCollectionFromAssignment(
        PropertyMappingSpec spec, string modelVarName, PropertyAnalysis analysis,
        GeneratorContext context, HelperMethodRegistry helperRegistry
    )
    {
        var collectionInfo = spec.TypeStrategy!.CollectionInfo!;
        var elementMapping = collectionInfo.ElementNestedMapping!;
        var itemParam = context.MapperOptions.FromMethodParameterName;
        var varName = spec.PropertyName.ToLowerInvariant();

        return collectionInfo.Category switch
        {
            CollectionCategory.List => RenderNestedListFromAssignment(
                spec,
                modelVarName,
                itemParam,
                varName,
                elementMapping,
                collectionInfo,
                context,
                helperRegistry
            ),
            CollectionCategory.Map => RenderNestedMapFromAssignment(
                spec,
                modelVarName,
                itemParam,
                varName,
                elementMapping,
                context,
                helperRegistry
            ),
            _ => throw new InvalidOperationException(
                $"Unexpected collection category for nested collection: {collectionInfo.Category}"
            ),
        };
    }

    /// <summary>
    ///     Renders FromItem regular assignment code for List&lt;NestedType&gt;.
    /// </summary>
    private static string RenderNestedListFromAssignment(
        PropertyMappingSpec spec, string modelVarName, string itemParam, string varName,
        NestedMappingInfo elementMapping, CollectionInfo collectionInfo, GeneratorContext context,
        HelperMethodRegistry helperRegistry
    )
    {
        var elementConverter =
            elementMapping switch
            {
                MapperBasedNesting mapperBased =>
                    $"{mapperBased.Mapper.MapperFullyQualifiedName}.FromItem(av.M)",
                InlineNesting inline =>
                    $"{helperRegistry.GetOrRegisterFromItemHelper(inline.Info.ModelFullyQualifiedType, inline.Info)}(av.M)",
                _ => throw new InvalidOperationException("Unknown nested mapping type"),
            };

        var selectExpr = $"{varName}List.Select(av => {elementConverter}).ToList()";

        // For arrays, add .ToArray()
        var toArray = collectionInfo.IsArray ? ".ToArray()" : "";

        return
            $"if ({itemParam}.TryGetValue(\"{spec.Key}\", out var {varName}Attr) && {varName}Attr.L is {{ }} {varName}List) {modelVarName}.{spec.PropertyName} = {selectExpr}{toArray};";
    }

    /// <summary>
    ///     Renders FromItem regular assignment code for Dictionary&lt;string, NestedType&gt;.
    /// </summary>
    private static string RenderNestedMapFromAssignment(
        PropertyMappingSpec spec, string modelVarName, string itemParam, string varName,
        NestedMappingInfo elementMapping, GeneratorContext context,
        HelperMethodRegistry helperRegistry
    )
    {
        var valueConverter =
            elementMapping switch
            {
                MapperBasedNesting mapperBased =>
                    $"{mapperBased.Mapper.MapperFullyQualifiedName}.FromItem(kvp.Value.M)",
                InlineNesting inline =>
                    $"{helperRegistry.GetOrRegisterFromItemHelper(inline.Info.ModelFullyQualifiedType, inline.Info)}(kvp.Value.M)",
                _ => throw new InvalidOperationException("Unknown nested mapping type"),
            };

        var toDictExpr = $"{varName}Map.ToDictionary(kvp => kvp.Key, kvp => {valueConverter})";

        return
            $"if ({itemParam}.TryGetValue(\"{spec.Key}\", out var {varName}Attr) && {varName}Attr.M is {{ }} {varName}Map) {modelVarName}.{spec.PropertyName} = {toDictExpr};";
    }

    #endregion
}
