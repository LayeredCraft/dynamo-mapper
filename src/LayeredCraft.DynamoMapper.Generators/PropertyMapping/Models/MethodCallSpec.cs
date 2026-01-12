namespace DynamoMapper.Generator.PropertyMapping.Models;

/// <summary>Specifies a method call with its name and arguments.</summary>
/// <param name="MethodName">The method name (e.g., "GetString", "SetInt").</param>
/// <param name="Arguments">The ordered list of arguments for the method call.</param>
internal sealed record MethodCallSpec(string MethodName, ArgumentSpec[] Arguments);
