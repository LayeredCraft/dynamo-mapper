namespace DynamoMapper.Runtime;

/// <summary>
///     Configures mapping behavior for a specific property/field on a mapper method. This
///     attribute can be applied multiple times to configure different properties.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class DynamoFieldAttribute : Attribute
{
    /// <summary>Initializes a new instance of the <see cref="DynamoFieldAttribute" /> class.</summary>
    /// <param name="memberName">
    ///     The name of the .NET property/member to configure. Use
    ///     <c>nameof(Type.Property)</c> for compile-time safety.
    /// </param>
    public DynamoFieldAttribute(string memberName) =>
        MemberName = memberName ?? throw new ArgumentNullException(nameof(memberName));

    /// <summary>Gets the .NET property/member name being configured.</summary>
    public string MemberName { get; }

    /// <summary>Gets or sets the DynamoDB attribute name override.</summary>
    /// <remarks>When <c>null</c>, the attribute name is determined by the mapper's naming convention.</remarks>
    public string AttributeName { get; set; } = null!;

    /// <summary>Gets or sets whether this field is required.</summary>
    /// <remarks>
    ///     When <c>null</c>, requiredness is determined by the mapper's default or property
    ///     nullability.
    /// </remarks>
    public bool Required { get; set; }

    /// <summary>Gets or sets the DynamoDB AttributeValue kind override.</summary>
    /// <remarks>When <c>null</c>, the kind is inferred from the property type.</remarks>
    public DynamoKind Kind { get; set; }

    /// <summary>Gets or sets whether to omit this field if the value is null.</summary>
    public bool OmitIfNull { get; set; }

    /// <summary>Gets or sets whether to omit this field if the value is null or whitespace (for strings).</summary>
    public bool OmitIfEmptyString { get; set; }

    /// <summary>Gets or sets the name of the static method to use for ToItem conversion.</summary>
    /// <remarks>
    ///     Method must have signature: <c>static AttributeValue MethodName(T value)</c>.
    /// </remarks>
    public string ToMethod { get; set; } = null!;

    /// <summary>Gets or sets the name of the static method to use for FromItem conversion.</summary>
    /// <remarks>
    ///     Method must have signature: <c>static T MethodName(AttributeValue value)</c>.
    /// </remarks>
    public string FromMethod { get; set; } = null!;

    /// <summary>Gets or sets the format string for types that support formatting.</summary>
    /// <remarks>
    ///     <para>Supported types and their format strings:</para>
    ///     <list type="bullet">
    ///         <item>
    ///             <term>DateTime / DateTimeOffset</term>
    ///             <description>Standard or custom format strings (e.g., "O", "yyyy-MM-dd", "MM/dd/yyyy")</description>
    ///         </item>
    ///         <item>
    ///             <term>TimeSpan</term>
    ///             <description>
    ///                 "c" (constant), "g" (general short), "G" (general long), or custom
    ///                 patterns
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term>Guid</term>
    ///             <description>
    ///                 "N" (32 digits), "D" (hyphens), "B" (braces), "P" (parentheses), "X"
    ///                 (hexadecimal)
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term>Enum</term>
    ///             <description>"G" (general), "F" (flags), "D" (decimal), "X" (hexadecimal)</description>
    ///         </item>
    ///     </list>
    ///     <para>
    ///         When set, this format overrides the mapper-level format (DateTimeFormat, TimeSpanFormat,
    ///         GuidFormat, EnumFormat).
    ///     </para>
    ///     <para>
    ///         For types that don't support formatting (string, bool, numeric types), this property is
    ///         ignored.
    ///     </para>
    /// </remarks>
    public string Format { get; set; } = null!;
}
