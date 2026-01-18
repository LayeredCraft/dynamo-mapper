namespace DynamoMapper.Runtime;

/// <summary>
///     Marks which constructor the DynamoMapper generator should use when deserializing entities
///     from DynamoDB AttributeValue dictionaries.
/// </summary>
[AttributeUsage(AttributeTargets.Constructor)]
public class DynamoMapperConstructorAttribute : Attribute;
