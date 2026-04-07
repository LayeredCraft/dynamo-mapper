using Amazon.DynamoDBv2.Model;

namespace LayeredCraft.DynamoMapper.Client.Models;

/// <summary>
///     Represents the result of a DynamoDB PartiQL <c>ExecuteStatement</c> operation together
///     with mapped DTO items.
/// </summary>
/// <typeparam name="T">The DTO type produced from the returned DynamoDB items.</typeparam>
/// <remarks>
///     This type provides the same general response context as
///     <see cref="Amazon.DynamoDBv2.Model.ExecuteStatementResponse" />, including the raw statement
///     results, pagination state, and consumed capacity details, while also exposing
///     <see cref="MappedItems" /> for typed access through a registered mapper.
/// </remarks>
public class ExecuteStatementResponse<T> : ExecuteStatementResponse
{
    internal ExecuteStatementResponse(ExecuteStatementResponse response, List<T> mappedItems)
    {
        MappedItems = mappedItems;
        Items = response.Items;
        LastEvaluatedKey = response.LastEvaluatedKey;
        NextToken = response.NextToken;
        ConsumedCapacity = response.ConsumedCapacity;
    }

    /// <summary>Gets the items returned by DynamoDB mapped to <typeparamref name="T" />.</summary>
    /// <remarks>
    ///     This list corresponds to the raw items exposed by
    ///     <see cref="Amazon.DynamoDBv2.Model.ExecuteStatementResponse.Items" />, but each entry has been
    ///     projected into the typed DTO using the registered mapper.
    /// </remarks>
    public List<T> MappedItems { get; }
}
