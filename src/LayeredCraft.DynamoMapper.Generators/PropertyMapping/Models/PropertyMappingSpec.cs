namespace DynamoMapper.Generator.PropertyMapping.Models;

/// <summary>
///     Complete specification of how to map a property to/from DynamoDB. Contains all information
///     needed to generate the mapping code.
/// </summary>
/// <param name="PropertyName">The property name.</param>
/// <param name="TypeStrategy">The type mapping strategy.</param>
/// <param name="ToItemMethod">Method specification for serialization (ToItem).</param>
/// <param name="FromItemMethod">Method specification for deserialization (FromItem).</param>
internal sealed record PropertyMappingSpec(
    string PropertyName,
    TypeMappingStrategy? TypeStrategy,
    MethodCallSpec ToItemMethod,
    MethodCallSpec FromItemMethod
);
