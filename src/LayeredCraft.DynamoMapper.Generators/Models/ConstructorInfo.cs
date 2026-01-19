using LayeredCraft.SourceGeneratorTools.Types;

namespace DynamoMapper.Generator.Models;

/// <summary>
///     Information about a constructor to use for deserialization.
/// </summary>
/// <param name="Parameters">The constructor parameters with their rendered argument expressions.</param>
internal sealed record ConstructorInfo(EquatableArray<ConstructorParameterInfo> Parameters);

/// <summary>
///     Information about a single constructor parameter.
/// </summary>
/// <param name="ParameterName">The parameter name (camelCase).</param>
/// <param name="RenderedArgument">The rendered argument expression (e.g., "item.GetString(\"name\", ...)").</param>
internal sealed record ConstructorParameterInfo(string ParameterName, string RenderedArgument);
