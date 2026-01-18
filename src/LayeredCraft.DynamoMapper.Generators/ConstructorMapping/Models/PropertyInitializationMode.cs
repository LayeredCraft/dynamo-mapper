namespace DynamoMapper.Generator.ConstructorMapping.Models;

/// <summary>
///     Describes how a specific property should be initialized when using constructor-based
///     instantiation.
/// </summary>
/// <param name="PropertyName">The name of the property.</param>
/// <param name="Method">The initialization method to use for this property.</param>
internal sealed record PropertyInitializationMode(string PropertyName, InitializationMethod Method);
