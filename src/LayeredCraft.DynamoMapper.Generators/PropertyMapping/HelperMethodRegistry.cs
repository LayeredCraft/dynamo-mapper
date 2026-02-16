using DynamoMapper.Generator.Models;
using DynamoMapper.Generator.PropertyMapping.Models;

namespace DynamoMapper.Generator.PropertyMapping;

/// <summary>
///     Registry for tracking helper methods during mapper generation. Ensures that the same
///     nested type reuses the same helper method.
/// </summary>
internal sealed class HelperMethodRegistry
{
    private readonly Dictionary<string, HelperMethodInfo> _toItemHelpers = new();
    private readonly Dictionary<string, HelperMethodInfo> _fromItemHelpers = new();

    /// <summary>
    ///     Gets or registers a ToItem helper method for a nested type. Returns the method name to use
    ///     in the call site.
    /// </summary>
    public string GetOrRegisterToItemHelper(
        string modelFullyQualifiedType, NestedInlineInfo inlineInfo
    )
    {
        if (_toItemHelpers.TryGetValue(modelFullyQualifiedType, out var existing))
            return existing.MethodName;

        var methodName = GenerateToItemHelperName(modelFullyQualifiedType);
        var helperInfo =
            new HelperMethodInfo(
                methodName,
                modelFullyQualifiedType,
                inlineInfo,
                HelperMethodDirection.ToItem
            );

        _toItemHelpers[modelFullyQualifiedType] = helperInfo;
        return methodName;
    }

    /// <summary>
    ///     Gets or registers a FromItem helper method for a nested type. Returns the method name to
    ///     use in the call site.
    /// </summary>
    public string GetOrRegisterFromItemHelper(
        string modelFullyQualifiedType, NestedInlineInfo inlineInfo
    )
    {
        if (_fromItemHelpers.TryGetValue(modelFullyQualifiedType, out var existing))
            return existing.MethodName;

        var methodName = GenerateFromItemHelperName(modelFullyQualifiedType);
        var helperInfo =
            new HelperMethodInfo(
                methodName,
                modelFullyQualifiedType,
                inlineInfo,
                HelperMethodDirection.FromItem
            );

        _fromItemHelpers[modelFullyQualifiedType] = helperInfo;
        return methodName;
    }

    /// <summary>Returns all registered helper methods for emission.</summary>
    public HelperMethodInfo[] GetAllHelpers() =>
        _toItemHelpers.Values.Concat(_fromItemHelpers.Values).ToArray();

    private static string GenerateToItemHelperName(string modelFullyQualifiedType)
    {
        // Extract simple type name from fully qualified name
        // "global::MyNamespace.Address" -> "Address"
        var typeName = modelFullyQualifiedType.Split('.').Last();

        // Sanitize generic types: Result<Address> -> Result_Address
        typeName = typeName.Replace("<", "_").Replace(">", "").Replace(",", "_").Replace(" ", "");

        return $"ToItem_{typeName}";
    }

    private static string GenerateFromItemHelperName(string modelFullyQualifiedType)
    {
        var typeName = modelFullyQualifiedType.Split('.').Last();

        typeName = typeName.Replace("<", "_").Replace(">", "").Replace(",", "_").Replace(" ", "");

        return $"FromItem_{typeName}";
    }
}
