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

    /// <summary>
    ///     Gets or sets the default format string for DateTime and DateTimeOffset properties.
    /// </summary>
    /// <remarks>Default is "O" (round-trip format, ISO-8601).</remarks>
    public string DateTimeFormat { get; set; } = "O";

    /// <summary>Gets or sets the default format string for TimeSpan properties.</summary>
    /// <remarks>Default is "c" (constant/invariant format).</remarks>
    public string TimeSpanFormat { get; set; } = "c";

    /// <summary>Gets or sets the default enum serialization format.</summary>
    /// <remarks>
    ///     <para>The default is "G" (General format), which serializes enums as their string name (e.g., "Active").</para>
    ///     <para>Valid format strings:</para>
    ///     <list type="bullet">
    ///         <item>"G" or "g" - General format (displays string name if defined, otherwise numeric value)</item>
    ///         <item>"F" or "f" - Flags format (treats enum as bit flags)</item>
    ///         <item>"D" or "d" - Decimal format (displays as integer value)</item>
    ///         <item>"X" or "x" - Hexadecimal format (displays as hex value)</item>
    ///     </list>
    /// </remarks>
    public string EnumFormat { get; set; } = "G";

    /// <summary>Gets or sets the default Guid format string.</summary>
    /// <remarks>
    ///     <para>The default is "D" (standard format with hyphens): 00000000-0000-0000-0000-000000000000</para>
    ///     <para>Valid format strings:</para>
    ///     <list type="bullet">
    ///         <item>"N" - 32 digits: 00000000000000000000000000000000</item>
    ///         <item>"D" - 32 digits separated by hyphens: 00000000-0000-0000-0000-000000000000</item>
    ///         <item>
    ///             "B" - 32 digits separated by hyphens, enclosed in braces:
    ///             {00000000-0000-0000-0000-000000000000}
    ///         </item>
    ///         <item>
    ///             "P" - 32 digits separated by hyphens, enclosed in parentheses:
    ///             (00000000-0000-0000-0000-000000000000)
    ///         </item>
    ///     </list>
    /// </remarks>
    public string GuidFormat { get; set; } = "D";
}
