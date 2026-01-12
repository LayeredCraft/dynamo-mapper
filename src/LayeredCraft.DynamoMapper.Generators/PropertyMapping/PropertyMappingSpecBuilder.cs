using DynamoMapper.Generator.PropertyMapping.Models;

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
        TypeMappingStrategy strategy,
        GeneratorContext context
    )
    {
        var key = context.MapperOptions.KeyNamingConventionConverter(analysis.PropertyName);

        var fromItemMethod = BuildFromItemMethod(analysis, strategy, key, context);
        var toItemMethod = BuildToItemMethod(analysis, strategy, key, context);

        return new PropertyMappingSpec(
            analysis.PropertyName,
            strategy,
            toItemMethod,
            fromItemMethod
        );
    }

    /// <summary>
    ///     Builds the method specification for deserialization (FromItem). Method name format:
    ///     Get{Nullable}{TypeName}{Generic} Arguments: [key, ...type-specific args, requiredness]
    /// </summary>
    private static MethodCallSpec BuildFromItemMethod(
        PropertyAnalysis analysis,
        TypeMappingStrategy strategy,
        string key,
        GeneratorContext context
    )
    {
        var methodName = $"Get{strategy.NullableModifier}{strategy.TypeName}";

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

        // Final argument: Requiredness
        args.Add(
            new ArgumentSpec(
                $"Requiredness.{context.MapperOptions.DefaultRequiredness}",
                ArgumentSource.GlobalOption
            )
        );

        return new MethodCallSpec(methodName, [.. args]);
    }

    /// <summary>
    ///     Builds the method specification for serialization (ToItem). Method name format:
    ///     Set{TypeName}{Generic} (no Nullable modifier) Arguments: [key, sourceProperty, ...type-specific
    ///     args, omitEmptyStrings, omitNullStrings]
    /// </summary>
    private static MethodCallSpec BuildToItemMethod(
        PropertyAnalysis analysis,
        TypeMappingStrategy strategy,
        string key,
        GeneratorContext context
    )
    {
        var methodName = $"Set{strategy.TypeName}";
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

        // Final arguments: Omit flags
        args.Add(
            new ArgumentSpec(
                context.MapperOptions.OmitEmptyStrings.ToString().ToLowerInvariant(),
                ArgumentSource.GlobalOption
            )
        );
        args.Add(
            new ArgumentSpec(
                context.MapperOptions.OmitNullStrings.ToString().ToLowerInvariant(),
                ArgumentSource.GlobalOption
            )
        );

        return new MethodCallSpec(methodName, [.. args]);
    }
}
