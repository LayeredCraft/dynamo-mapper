using Amazon.DynamoDBv2.Model;
using LayeredCraft.DynamoMapper.Runtime;

namespace LayeredCraft.DynamoMapper.IntegrationTests;

/// <summary>
///     Model for testing non-partial lifecycle hooks — same behaviour as partial hooks but using
///     ordinary <c>static void</c> methods instead of <c>static partial void</c>.
/// </summary>
public sealed class NonPartialHooksModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    /// <summary>Not mapped; populated by the <c>AfterFromItem</c> hook.</summary>
    public string NormalizedName { get; set; } = string.Empty;
}

[DynamoMapper]
[DynamoIgnore(nameof(NonPartialHooksModel.NormalizedName))]
public static partial class NonPartialHooksModelMapper
{
    public static partial Dictionary<string, AttributeValue> ToItem(NonPartialHooksModel source);
    public static partial NonPartialHooksModel FromItem(Dictionary<string, AttributeValue> item);

    private static void BeforeToItem(
        NonPartialHooksModel source, Dictionary<string, AttributeValue> item
    ) => item.SetString("_lifecyclePhase", "before-mapping");

    private static void AfterToItem(
        NonPartialHooksModel source, Dictionary<string, AttributeValue> item
    )
    {
        item.SetString("pk", $"MODEL#{source.Id}");
        item.SetString("sk", "METADATA");
        item.SetString("_lifecyclePhase", "after-mapping");
    }

    private static void BeforeFromItem(Dictionary<string, AttributeValue> item)
    {
        if (item.TryGetValue("entityType", out var typeAttr) &&
            typeAttr.S != "NonPartialHooksModel")
            throw new InvalidOperationException(
                $"BeforeFromItem: expected 'NonPartialHooksModel', got '{typeAttr.S}'"
            );
    }

    private static void AfterFromItem(
        Dictionary<string, AttributeValue> item, ref NonPartialHooksModel entity
    ) => entity.NormalizedName = entity.Name.ToUpperInvariant();
}
