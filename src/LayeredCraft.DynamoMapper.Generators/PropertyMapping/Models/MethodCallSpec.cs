namespace DynamoMapper.Generator.PropertyMapping.Models;

/// <summary>Specifies a method call with its name and arguments.</summary>
/// <param name="MethodName">The method name (e.g., "GetString", "SetInt").</param>
/// <param name="Arguments">The ordered list of arguments for the method call.</param>
/// <param name="IsCustomMethod">True if this is a user-defined custom method, false for standard extension methods.</param>
internal sealed record MethodCallSpec(
    string MethodName,
    ArgumentSpec[] Arguments,
    bool IsCustomMethod = false
);
