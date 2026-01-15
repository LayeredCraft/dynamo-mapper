namespace DynamoMapper.Generator.PropertyMapping.Models;

/// <summary>
///     Complete specification of how to map a property to/from DynamoDB. Contains all information
///     needed to generate the mapping code.
/// </summary>
/// <param name="PropertyName">The property name.</param>
/// <param name="TypeStrategy">The type mapping strategy.</param>
/// <param name="FromModelMethod">Method specification for serialization (FromModel - Model → DynamoDB), or null if mapper doesn't have FromModel.</param>
/// <param name="ToModelMethod">Method specification for deserialization (ToModel - DynamoDB → Model), or null if mapper doesn't have ToModel.</param>
internal sealed record PropertyMappingSpec(
    string PropertyName,
    TypeMappingStrategy? TypeStrategy,
    MethodCallSpec? FromModelMethod,
    MethodCallSpec? ToModelMethod
);
