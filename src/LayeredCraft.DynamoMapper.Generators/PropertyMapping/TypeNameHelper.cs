namespace DynamoMapper.Generator.PropertyMapping;

/// <summary>Utility for extracting and sanitizing type names from fully-qualified type strings.</summary>
internal static class TypeNameHelper
{
    /// <summary>
    ///     Extracts the simple type name from a fully-qualified type string, sanitized for use as a
    ///     C# identifier. Strips "global::" prefix, takes the last dot-delimited segment, and replaces
    ///     generic syntax characters. Example: "global::MyNamespace.Result&lt;Address&gt;" →
    ///     "Result_Address"
    /// </summary>
    public static string ExtractSanitized(string fullyQualifiedType)
    {
        var typeName = fullyQualifiedType.Replace("global::", "").Split('.').Last();
        return typeName.Replace("<", "_").Replace(">", "").Replace(",", "_").Replace(" ", "");
    }

    /// <summary>
    ///     Returns the sanitized type name as an all-lowercase string, suitable for use as a method
    ///     parameter name. Example: "global::MyNamespace.Address" → "address"
    /// </summary>
    public static string ToParameterName(string fullyQualifiedType) =>
        ExtractSanitized(fullyQualifiedType).ToLowerInvariant();

    /// <summary>
    ///     Returns the sanitized type name with the first letter lowercased, suitable for use as a
    ///     local variable name. Example: "global::MyNamespace.Address" → "address"
    /// </summary>
    public static string ToVariableName(string fullyQualifiedType)
    {
        var name = ExtractSanitized(fullyQualifiedType);
        return name.Length == 0 ? name : char.ToLowerInvariant(name[0]) + name.Substring(1);
    }
}
