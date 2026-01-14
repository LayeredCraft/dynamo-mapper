using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using DynamoMapper.Generator.PropertyMapping.Models;
using DynamoMapper.Runtime;

namespace DynamoMapper.Generator.PropertyMapping;

/// <summary>
///     Builds property mapping specifications with complete method call information. Constructs
///     Get/Set method calls with all arguments in the correct order.
/// </summary>
internal static class PropertyMappingSpecBuilder
{
    /// <summary>Builds a complete property mapping specification.</summary>
    /// <param name="analysis">The property analysis.</param>
    /// <param name="strategy">The type mapping strategy.</param>
    /// <param name="context">The generator context.</param>
    /// <returns>A property mapping specification ready for code generation.</returns>
    internal static PropertyMappingSpec Build(
        PropertyAnalysis analysis,
        TypeMappingStrategy? strategy,
        GeneratorContext context
    )
    {
        var fieldOptions = analysis.FieldOptions;

        // Handle custom methods first - they completely replace standard Get/Set methods
        if (fieldOptions?.ToMethod is not null || fieldOptions?.FromMethod is not null)
            return BuildWithCustomMethods(analysis, strategy, fieldOptions!, context);

        var key = GetAttributeKey(analysis, context);

        // Check if property should be ignored in specific directions
        var ignoreOptions = context.IgnoreOptions.TryGetValue(analysis.PropertyName, out var opts)
            ? opts
            : null;
        var shouldIgnoreToItem =
            ignoreOptions?.Ignore is IgnoreMapping.All or IgnoreMapping.FromModel;
        var shouldIgnoreFromItem =
            ignoreOptions?.Ignore is IgnoreMapping.All or IgnoreMapping.ToModel;

        // Only build methods if the mapper has them defined
        var fromItemMethod = context.HasFromItemMethod
            ? shouldIgnoreFromItem
                ? null
                : BuildFromItemMethod(analysis, strategy, key, context)
            : null;
        var toItemMethod = context.HasToItemMethod
            ? shouldIgnoreToItem
                ? null
                : BuildToItemMethod(analysis, strategy, key, context)
            : null;

        return new PropertyMappingSpec(
            analysis.PropertyName,
            strategy,
            toItemMethod,
            fromItemMethod
        );
    }

    /// <summary>
    ///     Gets the DynamoDB attribute key name for a property. Uses AttributeName override if
    ///     specified, otherwise applies the naming convention converter.
    /// </summary>
    private static string GetAttributeKey(PropertyAnalysis analysis, GeneratorContext context) =>
        analysis.FieldOptions?.AttributeName
        ?? context.MapperOptions.KeyNamingConventionConverter(analysis.PropertyName);

    /// <summary>
    ///     Builds the method specification for deserialization (FromItem). Method name format:
    ///     Get{Nullable}{TypeName}{Generic} Arguments: [key, ...type-specific args, requiredness,
    ///     (optional) kind]
    /// </summary>
    private static MethodCallSpec BuildFromItemMethod(
        PropertyAnalysis analysis,
        [NotNull] TypeMappingStrategy? strategy,
        string key,
        GeneratorContext context
    )
    {
        Debug.Assert(
            strategy is not null,
            "TypeMappingStrategy cannot be null for FromItem method"
        );

        var methodName = $"Get{strategy!.NullableModifier}{strategy.TypeName}";

        var args = new List<ArgumentSpec>
        {
            // Argument 1: The DynamoDB attribute key
            new($"\"{key}\"", ArgumentSource.Key),
        };

        // Arguments 2+: Type-specific arguments (format strings, default values)
        args.AddRange(
            strategy.FromTypeSpecificArgs.Select(typeArg => new ArgumentSpec(
                typeArg,
                ArgumentSource.TypeSpecific
            ))
        );

        // Requiredness: field override > global default
        var requiredness = analysis.FieldOptions?.Required switch
        {
            true => Requiredness.Required,
            false => Requiredness.Optional,
            null => context.MapperOptions.DefaultRequiredness,
        };

        args.Add(
            new ArgumentSpec(
                $"Requiredness.{requiredness}",
                analysis.FieldOptions?.Required is not null
                    ? ArgumentSource.FieldOverride
                    : ArgumentSource.GlobalOption
            )
        );

        // If Kind override exists, add it as final argument
        if (strategy.KindOverride is not null)
            args.Add(
                new ArgumentSpec(
                    $"DynamoKind.{strategy.KindOverride}",
                    ArgumentSource.FieldOverride
                )
            );

        return new MethodCallSpec(methodName, [.. args]);
    }

    /// <summary>
    ///     Builds the method specification for serialization (ToItem). Method name format:
    ///     Set{TypeName}{Generic} (no Nullable modifier) Arguments: [key, sourceProperty, ...type-specific
    ///     args, omitEmptyStrings, omitNullStrings, (optional) kind]
    /// </summary>
    private static MethodCallSpec BuildToItemMethod(
        PropertyAnalysis analysis,
        [NotNull] TypeMappingStrategy? strategy,
        string key,
        GeneratorContext context
    )
    {
        Debug.Assert(strategy is not null, "TypeMappingStrategy cannot be null for ToItem method");

        var methodName = $"Set{strategy!.TypeName}";
        var paramName = context.MapperOptions.ToMethodParameterName;

        var args = new List<ArgumentSpec>
        {
            // Argument 1: The DynamoDB attribute key
            new($"\"{key}\"", ArgumentSource.Key),
            // Argument 2: The property value from source object
            new($"{paramName}.{analysis.PropertyName}", ArgumentSource.PropertyValue),
        };

        // Arguments 3+: Type-specific arguments (format strings)
        args.AddRange(
            strategy.ToTypeSpecificArgs.Select(typeArg => new ArgumentSpec(
                typeArg,
                ArgumentSource.TypeSpecific
            ))
        );

        // Omit flags: field override > global default
        var omitEmptyStrings =
            analysis.FieldOptions?.OmitIfEmptyString ?? context.MapperOptions.OmitEmptyStrings;
        var omitNullStrings =
            analysis.FieldOptions?.OmitIfNull ?? context.MapperOptions.OmitNullStrings;

        args.Add(
            new ArgumentSpec(
                omitEmptyStrings.ToString().ToLowerInvariant(),
                analysis.FieldOptions?.OmitIfEmptyString is not null
                    ? ArgumentSource.FieldOverride
                    : ArgumentSource.GlobalOption
            )
        );
        args.Add(
            new ArgumentSpec(
                omitNullStrings.ToString().ToLowerInvariant(),
                analysis.FieldOptions?.OmitIfNull is not null
                    ? ArgumentSource.FieldOverride
                    : ArgumentSource.GlobalOption
            )
        );

        // If Kind override exists, add it as final argument
        if (strategy.KindOverride is not null)
            args.Add(
                new ArgumentSpec(
                    $"DynamoKind.{strategy.KindOverride}",
                    ArgumentSource.FieldOverride
                )
            );

        return new MethodCallSpec(methodName, [.. args]);
    }

    /// <summary>
    ///     Builds a property mapping spec when custom ToMethod or FromMethod is specified. Custom
    ///     methods completely replace the standard Get/Set method calls.
    /// </summary>
    private static PropertyMappingSpec BuildWithCustomMethods(
        PropertyAnalysis analysis,
        TypeMappingStrategy? strategy,
        DynamoFieldOptions fieldOptions,
        GeneratorContext context
    )
    {
        var key = GetAttributeKey(analysis, context);

        // Only build FromItem method if mapper has FromItem defined
        var fromItemMethod = context.HasFromItemMethod
            ? fieldOptions.FromMethod is not null
                ? BuildCustomFromItemMethod(fieldOptions.FromMethod, context)
                : BuildFromItemMethod(analysis, strategy, key, context)
            : null;

        // Only build ToItem method if mapper has ToItem defined
        var toItemMethod = context.HasToItemMethod
            ? fieldOptions.ToMethod is not null
                ? BuildCustomToItemMethod(fieldOptions.ToMethod, analysis, context)
                : BuildToItemMethod(analysis, strategy, key, context)
            : null;

        return new PropertyMappingSpec(
            analysis.PropertyName,
            strategy,
            toItemMethod,
            fromItemMethod
        );
    }

    /// <summary>
    ///     Builds a custom FromItem method call. Custom FromItem methods receive the entire item
    ///     dictionary as their only argument.
    /// </summary>
    private static MethodCallSpec BuildCustomFromItemMethod(
        string methodName,
        GeneratorContext context
    )
    {
        var args = new[]
        {
            new ArgumentSpec(
                context.MapperOptions.FromMethodParameterName,
                ArgumentSource.FieldOverride
            ),
        };
        return new MethodCallSpec(methodName, args, true);
    }

    /// <summary>
    ///     Builds a custom ToItem method call. Custom ToItem methods receive the entire source object
    ///     and return an AttributeValue to be used within a .Set() call.
    /// </summary>
    private static MethodCallSpec BuildCustomToItemMethod(
        string methodName,
        PropertyAnalysis analysis,
        GeneratorContext context
    )
    {
        var key = GetAttributeKey(analysis, context);
        var paramName = context.MapperOptions.ToMethodParameterName;

        var args = new[]
        {
            new ArgumentSpec($"\"{key}\"", ArgumentSource.Key),
            new ArgumentSpec($"{methodName}({paramName})", ArgumentSource.FieldOverride),
        };
        return new MethodCallSpec("Set", args, true);
    }
}
