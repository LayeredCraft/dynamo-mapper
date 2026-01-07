namespace DynamoMapper.Runtime;

/// <summary>
///     Specifies how property requiredness should be determined when mapping between .NET types
///     and DynamoDB items.
/// </summary>
public enum Requiredness
{
    /// <summary>
    ///     Infer requiredness from nullability annotations. Nullable properties are treated as
    ///     optional, non-nullable properties are treated as required.
    /// </summary>
    /// <remarks>
    ///     This is the default behavior. For reference types, uses nullable reference type
    ///     annotations (e.g., <c>string?</c> is optional, <c>string</c> is required). For value types,
    ///     uses nullable value types (e.g., <c>int?</c> is optional, <c>int</c> is required).
    /// </remarks>
    InferFromNullability,

    /// <summary>
    ///     All properties are required by default. Missing properties will cause an error during
    ///     deserialization from DynamoDB items.
    /// </summary>
    Required,

    /// <summary>
    ///     All properties are optional by default. Missing properties will be set to their default
    ///     values during deserialization from DynamoDB items.
    /// </summary>
    Optional,
}
