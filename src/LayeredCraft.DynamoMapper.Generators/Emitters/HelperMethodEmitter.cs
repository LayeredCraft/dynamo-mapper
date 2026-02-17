using System.Text;
using DynamoMapper.Generator.Models;
using DynamoMapper.Generator.PropertyMapping;

namespace DynamoMapper.Generator.Emitters;

/// <summary>Generates helper method code for nested object mapping.</summary>
internal static class HelperMethodEmitter
{
    /// <summary>Renders a ToItem helper method.</summary>
    public static string RenderToItemHelper(
        HelperMethodInfo helper, GeneratorContext context, HelperMethodRegistry helperRegistry
    )
    {
        var sb = new StringBuilder();
        var paramName = TypeNameHelper.ToParameterName(helper.ModelFullyQualifiedType);

        // Method signature with arrow syntax (no leading spaces - template handles base
        // indentation)
        sb.AppendLine(
            $"private static Dictionary<string, AttributeValue> {helper.MethodName}({helper.ModelFullyQualifiedType} {paramName}) =>"
        );
        sb.Append("    ");

        // Count properties that produce Set* calls for dictionary capacity pre-allocation
        var capacity =
            helper.InlineInfo.Properties.Count(
                p => p.NestedMapping is not null || (p.Strategy is not null && p.HasGetter)
            );
        sb.Append($"new Dictionary<string, AttributeValue>({capacity})");

        // Render each property call on its own indented line directly from structured data
        foreach (var prop in helper.InlineInfo.Properties)
        {
            var call =
                PropertyMappingCodeRenderer.RenderToItemPropertyCall(
                    prop,
                    paramName,
                    context,
                    helperRegistry
                );
            if (call is not null)
            {
                sb.AppendLine();
                sb.Append("        ");
                sb.Append(call);
            }
        }

        sb.Append(";");
        return sb.ToString();
    }

    /// <summary>Renders a FromItem helper method.</summary>
    public static string RenderFromItemHelper(
        HelperMethodInfo helper, GeneratorContext context, HelperMethodRegistry helperRegistry
    )
    {
        var sb = new StringBuilder();
        var mapParamName = "map";

        // Method signature with block syntax (no leading spaces - template handles base
        // indentation)
        sb.AppendLine(
            $"private static {helper.ModelFullyQualifiedType} {helper.MethodName}(Dictionary<string, AttributeValue> {mapParamName})"
        );
        sb.AppendLine("{");

        // Determine rendering path from structured data, without an intermediate string
        var hasPostConstructionProperties =
            helper.InlineInfo.Properties.Any(
                p => !p.IsRequired && !p.IsInitOnly && p.HasDefaultValue && p.HasSetter
            );

        if (!hasPostConstructionProperties)
        {
            // Simple path: all properties go into the object initializer — build directly
            sb.Append("    return ");
            sb.AppendLine($"new {helper.ModelFullyQualifiedType}");
            sb.AppendLine("    {");

            foreach (var prop in helper.InlineInfo.Properties)
            {
                sb.Append("        ");
                PropertyMappingCodeRenderer.RenderPropertyInitAssignment(
                    prop,
                    mapParamName,
                    sb,
                    context,
                    helperRegistry
                );
                sb.AppendLine();
            }

            sb.Append("    }");
            sb.AppendLine(";");
        }
        else
        {
            // Complex path: mix of init and post-construction assignments.
            // RenderInlineNestedFromItem already emits properly indented multi-line code.
            var bodyCode =
                PropertyMappingCodeRenderer.RenderInlineNestedFromItem(
                    mapParamName,
                    helper.InlineInfo,
                    context,
                    helperRegistry
                );
            sb.Append(bodyCode);
            sb.AppendLine();
        }

        sb.AppendLine("}");

        return sb.ToString();
    }
}
