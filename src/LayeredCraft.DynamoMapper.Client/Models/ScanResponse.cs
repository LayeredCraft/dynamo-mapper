using Amazon.DynamoDBv2.Model;

namespace LayeredCraft.DynamoMapper.Client.Models;

/// <summary>Represents the result of a DynamoDB <c>Scan</c> operation together with mapped DTO items.</summary>
/// <typeparam name="T">The DTO type produced from the returned DynamoDB items.</typeparam>
/// <remarks>
///     This type provides the same general response context as
///     <see cref="Amazon.DynamoDBv2.Model.ScanResponse" />, including the raw scan items, result
///     counts, pagination state, and consumed capacity details, while also exposing
///     <see cref="MappedItems" /> for typed access through a registered mapper.
/// </remarks>
public class ScanResponse<T> : ScanResponse
{
    internal ScanResponse(ScanResponse response, List<T> mappedItems)
    {
        MappedItems = mappedItems;
        Items = response.Items;
        Count = response.Count;
        LastEvaluatedKey = response.LastEvaluatedKey;
        ScannedCount = response.ScannedCount;
        ConsumedCapacity = response.ConsumedCapacity;
    }

    /// <summary>Gets the items returned by DynamoDB mapped to <typeparamref name="T" />.</summary>
    /// <remarks>
    ///     This list corresponds to the raw items exposed by
    ///     <see cref="Amazon.DynamoDBv2.Model.ScanResponse.Items" />, but each entry has been projected
    ///     into the typed DTO using the registered mapper.
    /// </remarks>
    public List<T> MappedItems { get; }
}
