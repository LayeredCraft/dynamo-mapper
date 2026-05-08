using Amazon.DynamoDBv2.Model;

namespace LayeredCraft.DynamoMapper.Client.Models;

/// <summary>
///     Represents the result of a DynamoDB <c>UpdateItem</c> operation together with optional
///     mapped returned attributes.
/// </summary>
/// <typeparam name="T">The DTO type produced from returned DynamoDB attributes.</typeparam>
public class UpdateItemResponse<T> : UpdateItemResponse
{
    internal UpdateItemResponse(UpdateItemResponse response, T? mappedItem)
    {
        MappedItem = mappedItem;
        Attributes = response.Attributes;
        ConsumedCapacity = response.ConsumedCapacity;
        ItemCollectionMetrics = response.ItemCollectionMetrics;
    }

    /// <summary>Gets the returned attributes mapped to <typeparamref name="T" /> when present.</summary>
    public T? MappedItem { get; }
}
