using Amazon.DynamoDBv2.Model;
using LayeredCraft.DynamoMapper.Runtime;

namespace LayeredCraft.DynamoMapper.IntegrationTests;

/// <summary>
/// Model for testing customization hooks. Has a computed property (<see cref="NormalizedName"/>)
/// that is ignored by the mapper and populated only via <c>AfterFromItem</c>.
/// </summary>
public sealed class HooksIntegrationModel
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Not mapped to DynamoDB. Populated by the <c>AfterFromItem</c> hook.
    /// </summary>
    public string NormalizedName { get; set; } = string.Empty;
}

[DynamoMapper]
[DynamoIgnore(nameof(HooksIntegrationModel.NormalizedName))]
public static partial class HooksIntegrationModelMapper
{
    public static partial Dictionary<string, AttributeValue> ToItem(HooksIntegrationModel source);

    public static partial HooksIntegrationModel FromItem(Dictionary<string, AttributeValue> item);

    // Hook declarations — implementations are below
    static partial void BeforeToItem(HooksIntegrationModel source, Dictionary<string, AttributeValue> item);

    static partial void AfterToItem(HooksIntegrationModel source, Dictionary<string, AttributeValue> item);

    static partial void BeforeFromItem(Dictionary<string, AttributeValue> item);

    static partial void AfterFromItem(Dictionary<string, AttributeValue> item, ref HooksIntegrationModel entity);
}

public static partial class HooksIntegrationModelMapper
{
    /// <summary>
    /// Runs before property mapping. Item is empty at this point.
    /// Adds a lifecycle marker to verify execution order.
    /// </summary>
    static partial void BeforeToItem(HooksIntegrationModel source, Dictionary<string, AttributeValue> item)
    {
        item.SetString("_lifecyclePhase", "before-mapping");
    }

    /// <summary>
    /// Runs after property mapping. Adds single-table design keys and overwrites the
    /// lifecycle marker to verify AfterToItem runs after BeforeToItem and property mapping.
    /// </summary>
    static partial void AfterToItem(HooksIntegrationModel source, Dictionary<string, AttributeValue> item)
    {
        // Inject single-table design keys — the primary AfterToItem use case
        item.SetString("pk", $"MODEL#{source.Id}");
        item.SetString("sk", "METADATA");

        // Overwrite the lifecycle marker set by BeforeToItem to prove AfterToItem runs after
        item.SetString("_lifecyclePhase", "after-mapping");
    }

    /// <summary>
    /// Runs before property mapping during FromItem. Validates the entity type
    /// so that items from different entity types are rejected early.
    /// </summary>
    static partial void BeforeFromItem(Dictionary<string, AttributeValue> item)
    {
        if (item.TryGetValue("entityType", out var typeAttr) && typeAttr.S != "HooksIntegrationModel")
            throw new InvalidOperationException(
                $"BeforeFromItem: expected entity type 'HooksIntegrationModel', got '{typeAttr.S}'"
            );
    }

    /// <summary>
    /// Runs after object construction. Populates the computed <see cref="HooksIntegrationModel.NormalizedName"/>
    /// property from the mapped <see cref="HooksIntegrationModel.Name"/> value.
    /// </summary>
    static partial void AfterFromItem(
        Dictionary<string, AttributeValue> item, ref HooksIntegrationModel entity
    )
    {
        entity.NormalizedName = entity.Name.ToUpperInvariant();
    }
}
