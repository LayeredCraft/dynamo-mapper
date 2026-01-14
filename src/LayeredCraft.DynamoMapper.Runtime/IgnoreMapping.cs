namespace DynamoMapper.Runtime;

/// <summary>
///     Specifies which mapping direction(s) should skip a property marked with
///     <see cref="DynamoIgnoreAttribute" />.
/// </summary>
public enum IgnoreMapping
{
    /// <summary>
    ///     Skip this property in both <c>ToItem</c> (model to DynamoDB) and <c>FromItem</c>
    ///     (DynamoDB to model) mappings.
    /// </summary>
    All,

    /// <summary>
    ///     Skip this property when mapping from DynamoDB to the .NET model (<c>FromItem</c>
    ///     method). The property will still be mapped when serializing to DynamoDB
    ///     (<c>ToItem</c> method).
    /// </summary>
    ToModel,

    /// <summary>
    ///     Skip this property when mapping from the .NET model to DynamoDB (<c>ToItem</c>
    ///     method). The property will still be mapped when deserializing from DynamoDB
    ///     (<c>FromItem</c> method).
    /// </summary>
    FromModel,
}
