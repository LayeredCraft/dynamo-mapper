namespace DynamoMapper.Generator.PropertyMapping.Models;

/// <summary>
///     Indicates where an argument value originates from. Used for tracing and enabling future
///     override logic.
/// </summary>
internal enum ArgumentSource
{
    /// <summary>The DynamoDB attribute key derived from naming convention.</summary>
    Key,

    /// <summary>The property value from the source object (e.g., source.PropertyName).</summary>
    PropertyValue,

    /// <summary>Type-specific argument like format strings or default values.</summary>
    TypeSpecific,

    /// <summary>Global option from MapperOptions (e.g., Requiredness, omit flags).</summary>
    GlobalOption,

    /// <summary>Override from DynamoFieldOptions (future Phase 2 feature).</summary>
    FieldOverride,
}
