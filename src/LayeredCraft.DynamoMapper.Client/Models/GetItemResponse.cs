using Amazon.DynamoDBv2.Model;

namespace LayeredCraft.DynamoMapper.Client.Models;

/// <summary>
///     Represents the result of a DynamoDB <c>GetItem</c> operation together with an optional
///     mapped DTO instance.
/// </summary>
/// <typeparam name="T">The DTO type produced from the returned DynamoDB item.</typeparam>
/// <remarks>
///     This type provides the same general response context as
///     <see cref="Amazon.DynamoDBv2.Model.GetItemResponse" />, including consumed capacity details and
///     the raw DynamoDB item, while also exposing <see cref="MappedItem" /> for typed access through a
///     registered mapper.
/// </remarks>
public class GetItemResponse<T> : GetItemResponse
{
    internal GetItemResponse(GetItemResponse response, T? mappedItem)
    {
        MappedItem = mappedItem;
        Item = response.Item;
        ConsumedCapacity = response.ConsumedCapacity;
        IsItemSet = response.IsItemSet;
    }

    /// <summary>Gets the item returned by DynamoDB mapped to <typeparamref name="T" />.</summary>
    /// <remarks>
    ///     This value is <see langword="null" /> when the response does not contain an item or when
    ///     the mapped type is nullable and the mapper produces a null value.
    /// </remarks>
    public T? MappedItem { get; }
}
