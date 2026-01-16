namespace DynamoMapper.Generator.PropertyMapping.Models;

/// <summary>
///     Complete specification of how to map a property to/from DynamoDB. Contains all information
///     needed to generate the mapping code.
/// </summary>
internal sealed record PropertyMappingSpec(
    string PropertyName,
    string Key,
    TypeMappingStrategy? TypeStrategy,
    MethodCallSpec? ToItemMethod,
    MethodCallSpec? FromItemMethod
);
