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
        var typeName = ExtractSimpleTypeName(helper.ModelFullyQualifiedType);
        var paramName = typeName.ToLowerInvariant();

        // Method signature with arrow syntax (no leading spaces - template handles base
        // indentation)
        sb.AppendLine(
            $"private static Dictionary<string, AttributeValue> {helper.MethodName}({helper.ModelFullyQualifiedType} {paramName}) =>"
        );
        sb.Append("    ");

        // Reuse existing RenderInlineNestedToItem logic
        var bodyCode =
            PropertyMappingCodeRenderer.RenderInlineNestedToItem(
                paramName,
                helper.InlineInfo,
                context,
                helperRegistry
            );

        // Format chained method calls on separate lines
        var formattedBody = FormatToItemChainedCalls(bodyCode);
        sb.Append(formattedBody);
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

        // Reuse existing RenderInlineNestedFromItem logic
        var bodyCode =
            PropertyMappingCodeRenderer.RenderInlineNestedFromItem(
                mapParamName,
                helper.InlineInfo,
                context,
                helperRegistry
            );

        // Check if body is simple expression or multi-statement block
        var isSimpleExpression = bodyCode.TrimStart().StartsWith("new ");

        if (isSimpleExpression)
        {
            // Simple expression: "new Type { ... }"
            sb.Append("    return ");
            var formattedBody = FormatFromItemObjectInitializer(bodyCode);
            sb.Append(formattedBody);
            sb.AppendLine(";");
        }
        else
        {
            // Multi-statement block: "var x = new Type { ... }; if (...) ...; return x;"
            // Body already contains proper indentation and return statement
            sb.Append(bodyCode);
            sb.AppendLine();
        }

        sb.AppendLine("}");

        return sb.ToString();
    }

    /// <summary>
    ///     Formats ToItem chained method calls on separate lines. Input: new Dictionary&lt;string,
    ///     AttributeValue&gt;().SetString(...).SetDecimal(...) Output: new Dictionary&lt;string,
    ///     AttributeValue&gt;(capacity) .SetString(...) .SetDecimal(...)
    /// </summary>
    private static string FormatToItemChainedCalls(string bodyCode)
    {
        // Count how many .Set* method calls there are for capacity pre-allocation
        var capacity = CountSetMethodCalls(bodyCode);

        var sb = new StringBuilder();
        sb.Append($"new Dictionary<string, AttributeValue>({capacity})");

        // Find each .Set* method call and put it on a new line
        var startIndex = FindNextSetMethodCall(bodyCode, 0);

        while (startIndex >= 0)
        {
            // Find the matching closing parenthesis for this method call
            var i = startIndex + 1; // skip the '.'

            // Find the opening parenthesis
            while (i < bodyCode.Length && bodyCode[i] != '(')
                i++;

            if (i >= bodyCode.Length)
                break;

            i++; // skip the '('
            var parenCount = 1;

            // Find the matching closing parenthesis
            while (i < bodyCode.Length && parenCount > 0)
            {
                if (bodyCode[i] == '(')
                    parenCount++;
                else if (bodyCode[i] == ')')
                    parenCount--;
                i++;
            }

            // Extract the method call
            var methodCall = bodyCode.Substring(startIndex, i - startIndex);
            sb.AppendLine();
            sb.Append("        ");
            sb.Append(methodCall);

            // Find the next .Set call
            startIndex = FindNextSetMethodCall(bodyCode, i);
        }

        return sb.ToString();
    }

    /// <summary>
    ///     Formats FromItem object initializer on separate lines. Input: new MyType { Prop1 = val1,
    ///     Prop2 = val2, } Output: new MyType { Prop1 = val1, Prop2 = val2, }
    /// </summary>
    private static string FormatFromItemObjectInitializer(string bodyCode)
    {
        // Find the opening brace
        var braceIndex = bodyCode.IndexOf('{');
        if (braceIndex < 0)
            return bodyCode; // No object initializer, return as-is

        var sb = new StringBuilder();

        // Type name before the brace
        var typePart = bodyCode.Substring(0, braceIndex).TrimEnd();
        sb.AppendLine(typePart);
        sb.AppendLine("    {");

        // Extract the properties inside the braces
        var propsStart = braceIndex + 1;
        var propsEnd = bodyCode.LastIndexOf('}');
        if (propsEnd < 0)
            return bodyCode; // Malformed, return as-is

        var propsContent = bodyCode.Substring(propsStart, propsEnd - propsStart).Trim();

        // Split on commas, but need to be careful about nested commas
        var properties = SplitPropertiesRespectingNesting(propsContent);

        for (var i = 0; i < properties.Length; i++)
        {
            var trimmed = properties[i].Trim();
            if (!string.IsNullOrEmpty(trimmed))
            {
                sb.Append("        ");
                sb.Append(trimmed);
                // Add comma after each property (including the last one for trailing comma style)
                if (!trimmed.EndsWith(","))
                    sb.Append(",");
                sb.AppendLine();
            }
        }

        sb.Append("    }");

        return sb.ToString();
    }

    /// <summary>Counts the number of .Set* method calls in the body code for dictionary capacity.</summary>
    private static int CountSetMethodCalls(string bodyCode)
    {
        var count = 0;
        var searchFrom = 0;

        while ((searchFrom = FindNextSetMethodCall(bodyCode, searchFrom)) >= 0)
        {
            count++;
            searchFrom += 4; // Move past ".Set"
        }

        return count;
    }

    /// <summary>
    ///     Finds the next .Set* method call (not a property access like source.Settings) starting at
    ///     startFrom. Returns the index of the leading '.' or -1 if none found.
    /// </summary>
    private static int FindNextSetMethodCall(string bodyCode, int startFrom)
    {
        var index = startFrom;

        while ((index = bodyCode.IndexOf(".Set", index, StringComparison.Ordinal)) >= 0)
        {
            // Advance past ".Set" and any remaining identifier chars (the method name suffix)
            var nameEnd = index + 4;
            while (nameEnd < bodyCode.Length &&
                   (char.IsLetterOrDigit(bodyCode[nameEnd]) || bodyCode[nameEnd] == '_'))
                nameEnd++;

            // Skip generic type parameters, e.g. <string> in .SetList<string>(...)
            if (nameEnd < bodyCode.Length && bodyCode[nameEnd] == '<')
            {
                var depth = 1;
                nameEnd++;
                while (nameEnd < bodyCode.Length && depth > 0)
                {
                    if (bodyCode[nameEnd] == '<') depth++;
                    else if (bodyCode[nameEnd] == '>') depth--;
                    nameEnd++;
                }
            }

            // A method call must be immediately followed by '('
            if (nameEnd < bodyCode.Length && bodyCode[nameEnd] == '(')
                return index;

            // Not a method call (e.g. source.Settings); resume search after this position
            index += 4;
        }

        return -1;
    }

    /// <summary>Splits property assignments by comma, respecting nested parentheses.</summary>
    private static string[] SplitPropertiesRespectingNesting(string content)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        var depth = 0;

        for (var i = 0; i < content.Length; i++)
        {
            var ch = content[i];

            if (ch == '(' || ch == '<' || ch == '{')
            {
                depth++;
            }
            else if (ch == ')' || ch == '>' || ch == '}')
            {
                depth--;
            }
            else if (ch == ',' && depth == 0)
            {
                result.Add(current.ToString());
                current.Clear();
                continue;
            }

            current.Append(ch);
        }

        if (current.Length > 0)
            result.Add(current.ToString());

        return result.ToArray();
    }

    private static string ExtractSimpleTypeName(string fullyQualifiedType) =>
        // "global::MyNamespace.Address" -> "address"
        fullyQualifiedType.Split('.').Last().ToLowerInvariant();
}
