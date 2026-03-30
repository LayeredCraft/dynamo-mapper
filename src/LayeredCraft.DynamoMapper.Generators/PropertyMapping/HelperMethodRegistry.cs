using LayeredCraft.DynamoMapper.Generator.Models;
using LayeredCraft.DynamoMapper.Generator.PropertyMapping.Models;

namespace LayeredCraft.DynamoMapper.Generator.PropertyMapping;

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

        var methodName = GenerateHelperName(modelFullyQualifiedType, "ToItem");
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

        var methodName = GenerateHelperName(modelFullyQualifiedType, "FromItem");
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

    private static string GenerateHelperName(string modelFullyQualifiedType, string prefix) =>
        $"{prefix}_{TypeNameHelper.ExtractSanitized(modelFullyQualifiedType)}";
}
