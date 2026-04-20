using LayeredCraft.DynamoMapper.Generator.PropertyMapping.Models;
using LayeredCraft.DynamoMapper.Runtime;

namespace LayeredCraft.DynamoMapper.Generator.PropertyMapping;

/// <summary>Resolves effective requiredness for generated read paths.</summary>
internal static class RequirednessResolver
{
    /// <summary>Resolves configured requiredness for a property before nullability inference.</summary>
    internal static Requiredness ResolveConfigured(
        DynamoFieldOptions? fieldOptions, bool isCSharpRequired,
        Requiredness mapperDefaultRequiredness
    ) => fieldOptions?.Required switch
    {
        true => Requiredness.Required,
        false => Requiredness.Optional,
        null when isCSharpRequired => Requiredness.Required,
        null => mapperDefaultRequiredness,
    };

    /// <summary>Resolves whether a property is effectively required after nullability inference.</summary>
    internal static bool IsEffectivelyRequired(
        Requiredness configuredRequiredness, PropertyNullabilityInfo nullability
    ) => configuredRequiredness switch
    {
        Requiredness.Required => true,
        Requiredness.Optional => false,
        Requiredness.InferFromNullability => !nullability.IsNullableType,
        _ => throw new ArgumentOutOfRangeException(
            nameof(configuredRequiredness),
            configuredRequiredness,
            null
        ),
    };
}
