namespace DynamoMapper.Generator.PropertyMapping.Models;

/// <summary>Represents a single argument in a method call.</summary>
/// <param name="Value">The argument value as a string (e.g., "\"key\"", "source.Name", "true").</param>
/// <param name="Source">Where this argument originates from.</param>
internal sealed record ArgumentSpec(string Value, ArgumentSource Source);
