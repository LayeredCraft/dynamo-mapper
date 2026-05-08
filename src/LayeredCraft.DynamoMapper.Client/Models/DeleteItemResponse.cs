using Amazon.DynamoDBv2.Model;

namespace LayeredCraft.DynamoMapper.Client.Models;

/// <summary>
///     Represents the result of a DynamoDB <c>DeleteItem</c> operation together with optional
///     mapped returned attributes.
/// </summary>
/// <typeparam name="T">The DTO type produced from returned DynamoDB attributes.</typeparam>
/// <remarks>
///     DynamoDB only returns attributes for <c>DeleteItem</c> when
///     <see cref="DeleteItemRequest.ReturnValues" /> requests old values. In all other cases
///     <see cref="MappedItem" /> is <see langword="null" />.
/// </remarks>
public class DeleteItemResponse<T> : DeleteItemResponse
{
    internal DeleteItemResponse(DeleteItemResponse response, T? mappedItem)
    {
        MappedItem = mappedItem;
        Attributes = response.Attributes;
        ConsumedCapacity = response.ConsumedCapacity;
        ItemCollectionMetrics = response.ItemCollectionMetrics;
    }

    /// <summary>Gets the returned attributes mapped to <typeparamref name="T" /> when present.</summary>
    public T? MappedItem { get; }
}
