using System.Diagnostics;
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
        GeneratorContext context
    )
    {
        // Determine if we should use regular assignment vs init assignment
        var useRegularAssignment =
            spec.FromItemMethod is { IsCustomMethod: false }
            && analysis is { IsRequired: false, IsInitOnly: false, HasDefaultValue: true };

        // FromItem requires both: setter on property AND FromItem method exists
        var fromAssignment =
            context.HasFromItemMethod
            && analysis.HasSetter
            && spec.FromItemMethod is not null
            && useRegularAssignment
                ? RenderFromAssignment(spec, modelVarName, analysis, index, context)
                : null;

        var fromInitAssignment =
            context.HasFromItemMethod
            && analysis.HasSetter
            && spec.FromItemMethod is not null
            && !useRegularAssignment
                ? RenderFromInitAssignment(spec, analysis, context)
                : null;

        // ToItem requires both: getter on property AND ToItem method exists
        var toAssignments =
            context.HasToItemMethod && analysis.HasGetter && spec.ToItemMethod is not null
                ? RenderToAssignment(spec)
                : null;

        return new PropertyInfo(fromAssignment, fromInitAssignment, toAssignments);
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
        argsList.Insert(1, $"out var var{index}");
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
}
