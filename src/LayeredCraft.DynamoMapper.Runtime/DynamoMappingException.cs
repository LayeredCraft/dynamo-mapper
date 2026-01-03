namespace DynamoMapper.Runtime;

/// <summary>Exception thrown when DynamoDB mapping operations fail.</summary>
public class DynamoMappingException : Exception
{
    /// <summary>Gets the name of the mapper type where the error occurred.</summary>
    public string? Mapper { get; }

    /// <summary>Gets the target type being mapped.</summary>
    public string? TargetType { get; }

    /// <summary>Gets the DynamoDB field name associated with the error.</summary>
    public string? FieldName { get; }

    /// <summary>Gets the .NET property/member name associated with the error.</summary>
    public string? MemberName { get; }

    /// <summary>Gets additional error details.</summary>
    public string? Details { get; }

    /// <summary>Initializes a new instance of the <see cref="DynamoMappingException" /> class.</summary>
    public DynamoMappingException() { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DynamoMappingException" /> class with a
    ///     specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public DynamoMappingException(string message)
        : base(message) { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DynamoMappingException" /> class with a
    ///     specified error message and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public DynamoMappingException(string message, Exception innerException)
        : base(message, innerException) { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DynamoMappingException" /> class with
    ///     detailed mapping context.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="mapper">The mapper type name.</param>
    /// <param name="targetType">The target type being mapped.</param>
    /// <param name="fieldName">The DynamoDB field name.</param>
    /// <param name="memberName">The .NET property/member name.</param>
    /// <param name="details">Additional error details.</param>
    public DynamoMappingException(
        string message,
        string? mapper = null,
        string? targetType = null,
        string? fieldName = null,
        string? memberName = null,
        string? details = null
    )
        : base(message)
    {
        Mapper = mapper;
        TargetType = targetType;
        FieldName = fieldName;
        MemberName = memberName;
        Details = details;
    }
}
