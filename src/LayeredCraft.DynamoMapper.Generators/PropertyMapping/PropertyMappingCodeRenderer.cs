using System.Diagnostics;
using DynamoMapper.Generator.Models;
using DynamoMapper.Generator.PropertyMapping.Models;

namespace DynamoMapper.Generator.PropertyMapping;

/// <summary>
///     Renders property mapping specifications into PropertyInfo. Generates the final
///     FromAssignment and ToAssignments strings for the Scriban template.
/// </summary>
internal static class PropertyMappingCodeRenderer
{
    /// <summary>Renders a property mapping specification into PropertyInfo.</summary>
    /// <param name="spec">The property mapping specification.</param>
    /// <param name="analysis">The property analysis containing accessor information.</param>
    /// <param name="context">The generator context.</param>
    /// <returns>The rendered PropertyInfo.</returns>
    internal static PropertyInfo Render(
        PropertyMappingSpec spec,
        PropertyAnalysis analysis,
        GeneratorContext context
    )
    {
        // ToModel (DynamoDB → Model) requires both: setter on property AND ToModel method exists
        var toModelAssignment =
            context.HasToModelMethod && analysis.HasSetter && spec.ToModelMethod is not null
                ? RenderToModelAssignment(spec, context)
                : null;

        // FromModel (Model → DynamoDB) requires both: getter on property AND FromModel method
        // exists
        var fromModelAssignments =
            context.HasFromModelMethod && analysis.HasGetter && spec.FromModelMethod is not null
                ? RenderFromModelAssignment(spec)
                : null;

        return new PropertyInfo(toModelAssignment, fromModelAssignments);
    }

    /// <summary>
    ///     Renders the ToModel assignment string for deserialization (DynamoDB → Model). Format: PropertyName =
    ///     paramName.MethodName&lt;Generic&gt;(args), OR PropertyName = CustomMethodName(args), for
    ///     custom methods
    /// </summary>
    private static string RenderToModelAssignment(
        PropertyMappingSpec spec,
        GeneratorContext context
    )
    {
        Debug.Assert(spec.ToModelMethod is not null, "ToModelMethod should not be null");
        Debug.Assert(
            spec.ToModelMethod!.IsCustomMethod || spec.TypeStrategy is not null,
            "TypeStrategy should not be null for standard methods"
        );

        var args = string.Join(", ", spec.ToModelMethod.Arguments.Select(a => a.Value));

        var methodCall = spec.ToModelMethod.IsCustomMethod
            ? $"{spec.ToModelMethod.MethodName}({args})" // Custom: MethodName(item)
            : $"{context.MapperOptions.ToModelParameterName}.{spec.ToModelMethod.MethodName}{spec.TypeStrategy!.GenericArgument}({args})"; // Standard: item.GetXxx<T>(args)

        return $"{spec.PropertyName} = {methodCall},";
    }

    /// <summary>
    ///     Renders the FromModel assignment string for serialization (Model → DynamoDB). Format: .MethodName&lt;Generic&gt;(args)
    ///     Custom FromModel methods are rendered as .Set("key", CustomMethod(source))
    /// </summary>
    private static string RenderFromModelAssignment(PropertyMappingSpec spec)
    {
        Debug.Assert(spec.FromModelMethod is not null, "FromModelMethod should not be null");
        Debug.Assert(
            spec.FromModelMethod!.IsCustomMethod || spec.TypeStrategy is not null,
            "TypeStrategy should not be null for standard methods"
        );

        var args = string.Join(", ", spec.FromModelMethod.Arguments.Select(a => a.Value));

        var methodCall = spec.FromModelMethod.IsCustomMethod
            ? $".{spec.FromModelMethod.MethodName}({args})" // Custom: .Set("key",
            // CustomMethod(source))
            : $".{spec.FromModelMethod.MethodName}{spec.TypeStrategy!.GenericArgument}({args})"; // Standard: .SetXxx<T>(args)

        return methodCall;
    }
}
