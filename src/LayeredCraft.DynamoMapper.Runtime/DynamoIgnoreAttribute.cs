namespace DynamoMapper.Runtime;

/// <summary>
///     Excludes a property from being mapped in one or both directions. Applied to mapper
///     classes, this attribute can be specified multiple times to ignore different properties.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public sealed class DynamoIgnoreAttribute : Attribute
{
    /// <summary>Initializes a new instance of the <see cref="DynamoIgnoreAttribute" /> class.</summary>
    /// <param name="memberName">
    ///     The name of the .NET property/member to configure. Use
    ///     <c>nameof(Type.Property)</c> for compile-time safety.
    /// </param>
    public DynamoIgnoreAttribute(string memberName) =>
        MemberName = memberName ?? throw new ArgumentNullException(nameof(memberName));

    /// <summary>Gets the .NET property/member name being configured.</summary>
    public string MemberName { get; }

    /// <summary>
    ///     Gets or sets which mapping direction(s) should skip this property.
    /// </summary>
    /// <remarks>Default is <see cref="Runtime.SkipMapping.All" /> (skip in both directions).</remarks>
    public SkipMapping SkipMapping { get; set; } = SkipMapping.All;
}
