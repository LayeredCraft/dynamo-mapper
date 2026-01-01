namespace DynamoMapper.Runtime;

/// <summary>Specifies how enum values should be serialized when mapping to DynamoDB items.</summary>
public enum EnumFormat
{
    /// <summary>Serialize enums as their string name representation.</summary>
    /// <remarks>
    ///     This is the default behavior. For example, <c>DayOfWeek.Monday</c> would be serialized as
    ///     the string <c>"Monday"</c>. This format is more readable and resilient to enum value
    ///     reordering, but uses more storage space.
    /// </remarks>
    /// <example>
    ///     <code>
    ///     public enum Status { Active, Inactive, Archived }
    ///
    ///     // With EnumFormat.Name:
    ///     // Status.Active → "Active"
    ///     // Status.Archived → "Archived"
    ///     </code>
    /// </example>
    Name,

    /// <summary>Serialize enums as their underlying numeric value.</summary>
    /// <remarks>
    ///     For example, <c>DayOfWeek.Monday</c> (which has value 1) would be serialized as the number
    ///     <c>1</c>. This format is more compact and efficient, but less readable and can break if enum
    ///     values are reordered.
    /// </remarks>
    /// <example>
    ///     <code>
    ///     public enum Status { Active = 0, Inactive = 1, Archived = 2 }
    ///
    ///     // With EnumFormat.Numeric:
    ///     // Status.Active → 0
    ///     // Status.Inactive → 1
    ///     // Status.Archived → 2
    ///     </code>
    /// </example>
    Numeric,
}
