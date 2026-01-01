namespace DynamoMapper.Runtime;

/// <summary>
///     Marks a static partial class as a DynamoDB mapper and specifies mapper-level
///     configuration.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class DynamoMapperAttribute : Attribute
{
    /// <summary>
    ///     Gets or sets the naming convention for mapping .NET property names to DynamoDB attribute
    ///     names.
    /// </summary>
    /// <remarks>Default is <see cref="DynamoNamingConvention.CamelCase" />.</remarks>
    public DynamoNamingConvention Convention { get; set; } = DynamoNamingConvention.CamelCase;

    /// <summary>
    ///     Gets or sets the default requiredness behavior for properties.
    /// </summary>
    /// <remarks>
    ///     Default is <see cref="Requiredness.InferFromNullability" />.
    /// </remarks>
    public Requiredness DefaultRequiredness { get; set; } = Requiredness.InferFromNullability;

    /// <summary>Gets or sets whether to omit null string values from the DynamoDB item.</summary>
    /// <remarks>Default is <c>true</c>.</remarks>
    public bool OmitNullStrings { get; set; } = true;

    /// <summary>Gets or sets whether to omit empty string values from the DynamoDB item.</summary>
    /// <remarks>Default is <c>false</c>.</remarks>
    public bool OmitEmptyStrings { get; set; } = false;

    /// <summary>Gets or sets the default DateTime format string.</summary>
    /// <remarks>Default is "O" (round-trip format, ISO-8601).</remarks>
    public string DateTimeFormat { get; set; } = "O";

    /// <summary>Gets or sets the default enum serialization format.</summary>
    /// <remarks>
    ///     Default is <see cref="EnumFormat.Name" />, which serializes enums as their string name
    ///     (e.g., "Active"). Use <see cref="EnumFormat.Numeric" /> to serialize as the underlying numeric
    ///     value (e.g., 0).
    /// </remarks>
    public EnumFormat EnumFormat { get; set; } = EnumFormat.Name;
}
