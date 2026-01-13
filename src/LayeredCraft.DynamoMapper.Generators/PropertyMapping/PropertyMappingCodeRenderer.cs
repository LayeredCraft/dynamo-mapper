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
        // FromItem requires setter - property must be assignable
        var fromAssignment = analysis.HasSetter ? RenderFromAssignment(spec, context) : null;

        // ToItem requires getter - property must be readable
        var toAssignments = analysis.HasGetter ? RenderToAssignment(spec) : null;

        return new PropertyInfo(fromAssignment, toAssignments);
    }

    /// <summary>
    ///     Renders the FromAssignment string for deserialization. Format: PropertyName =
    ///     paramName.MethodName&lt;Generic&gt;(args), OR PropertyName = CustomMethodName(args), for
    ///     custom methods
    /// </summary>
    private static string RenderFromAssignment(PropertyMappingSpec spec, GeneratorContext context)
    {
        Debug.Assert(
            spec.FromItemMethod.IsCustomMethod || spec.TypeStrategy is not null,
            "TypeStrategy should not be null for standard methods"
        );

        var args = string.Join(", ", spec.FromItemMethod.Arguments.Select(a => a.Value));

        var methodCall = spec.FromItemMethod.IsCustomMethod
            ? $"{spec.FromItemMethod.MethodName}({args})" // Custom: MethodName(item)
            : $"{context.MapperOptions.FromMethodParameterName}.{spec.FromItemMethod.MethodName}{spec.TypeStrategy!.GenericArgument}({args})"; // Standard: item.GetXxx<T>(args)

        return $"{spec.PropertyName} = {methodCall},";
    }

    /// <summary>
    ///     Renders the ToAssignment string for serialization. Format: .MethodName&lt;Generic&gt;(args)
    ///     Custom ToMethods are rendered as .Set("key", CustomMethod(source))
    /// </summary>
    private static string RenderToAssignment(PropertyMappingSpec spec)
    {
        Debug.Assert(
            spec.ToItemMethod.IsCustomMethod || spec.TypeStrategy is not null,
            "TypeStrategy should not be null for standard methods"
        );

        var args = string.Join(", ", spec.ToItemMethod.Arguments.Select(a => a.Value));

        var methodCall = spec.ToItemMethod.IsCustomMethod
            ? $".{spec.ToItemMethod.MethodName}({args})" // Custom: .Set("key",
            // CustomMethod(source))
            : $".{spec.ToItemMethod.MethodName}{spec.TypeStrategy!.GenericArgument}({args})"; // Standard: .SetXxx<T>(args)

        return methodCall;
    }
}
