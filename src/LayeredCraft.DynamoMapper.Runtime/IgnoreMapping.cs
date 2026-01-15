namespace DynamoMapper.Runtime;

/// <summary>
///     Specifies which mapping direction(s) should skip a property marked with
///     <see cref="DynamoIgnoreAttribute" />.
/// </summary>
public enum IgnoreMapping
{
    /// <summary>
    ///     Skip this property in both <c>FromModel</c> (model to DynamoDB) and <c>ToModel</c>
    ///     (DynamoDB to model) mappings.
    /// </summary>
    All,

    /// <summary>
    ///     Skip this property when mapping from DynamoDB to the .NET model (<c>ToModel</c>
    ///     method). The property will still be mapped when serializing to DynamoDB
    ///     (<c>FromModel</c> method).
    /// </summary>
    ToModel,

    /// <summary>
    ///     Skip this property when mapping from the .NET model to DynamoDB (<c>FromModel</c>
    ///     method). The property will still be mapped when deserializing from DynamoDB
    ///     (<c>ToModel</c> method).
    /// </summary>
    FromModel,
}
