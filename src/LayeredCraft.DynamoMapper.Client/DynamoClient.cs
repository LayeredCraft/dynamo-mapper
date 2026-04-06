using System.Collections.Immutable;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

// ReSharper disable MemberCanBePrivate.Global

namespace LayeredCraft.DynamoMapper.Client;

/// <summary>
///     Provides typed convenience methods for reading and writing DynamoDB items through
///     registered mappers.
/// </summary>
public class DynamoClient
{
    private readonly ImmutableDictionary<Type, object> _mappers;

    /// <summary>Gets the underlying DynamoDB client used for all requests.</summary>
    public IAmazonDynamoDB AmazonDynamoDb { get; }

    internal DynamoClient(ImmutableDictionary<Type, object> mappers, IAmazonDynamoDB dynamoDbClient)
    {
        ArgumentNullException.ThrowIfNull(dynamoDbClient);

        _mappers = mappers;
        AmazonDynamoDb = dynamoDbClient;
    }

    /// <summary>Gets the mapper registered for the specified DTO type.</summary>
    /// <typeparam name="T">The DTO type to retrieve a mapper for.</typeparam>
    /// <returns>The mapper registered for <typeparamref name="T" />.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when no mapper has been registered for
    ///     <typeparamref name="T" />.
    /// </exception>
    public IDynamoMapper<T> GetMapper<T>()
    {
        if (_mappers.TryGetValue(typeof(T), out var mapper))
            return (IDynamoMapper<T>)mapper;

        throw new InvalidOperationException($"No mapper found for type {typeof(T)}");
    }

    /// <summary>Retrieves a single item by key and maps it to the specified DTO type.</summary>
    /// <typeparam name="T">The DTO type to map the item to.</typeparam>
    /// <param name="tableName">The DynamoDB table name.</param>
    /// <param name="key">The primary key of the item to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token for the asynchronous operation.</param>
    /// <returns>
    ///     A task that returns the mapped DTO when an item is found; otherwise,
    ///     <see langword="null" />.
    /// </returns>
    public async Task<T?> GetItemAsync<T>(
        string tableName,
        Dictionary<string, AttributeValue> key,
        CancellationToken cancellationToken = default)
    {
        var result = await AmazonDynamoDb.GetItemAsync(tableName, key, cancellationToken);
        return result.Item is null || result.Item.Count == 0
            ? default
            : GetMapper<T>().FromItem(result.Item);
    }

    /// <summary>Saves a mapped DTO to the specified table.</summary>
    /// <typeparam name="T">The DTO type to write.</typeparam>
    /// <param name="tableName">The DynamoDB table name.</param>
    /// <param name="item">The DTO instance to map and save.</param>
    /// <param name="cancellationToken">The cancellation token for the asynchronous operation.</param>
    /// <returns>A task that completes when the item has been written.</returns>
    public Task PutItemAsync<T>(
        string tableName,
        T item,
        CancellationToken cancellationToken = default)
    {
        var mappedItem = GetMapper<T>().ToItem(item);
        return AmazonDynamoDb.PutItemAsync(tableName, mappedItem, cancellationToken);
    }

    /// <summary>Deletes a single item by key from the specified table.</summary>
    /// <param name="tableName">The DynamoDB table name.</param>
    /// <param name="key">The primary key of the item to delete.</param>
    /// <param name="cancellationToken">The cancellation token for the asynchronous operation.</param>
    /// <returns>A task that completes when the delete request has finished.</returns>
    public Task DeleteItemAsync(
        string tableName,
        Dictionary<string, AttributeValue> key,
        CancellationToken cancellationToken = default)
        => AmazonDynamoDb.DeleteItemAsync(tableName, key, cancellationToken);

    /// <summary>Executes an update request and maps returned attributes to the specified DTO type.</summary>
    /// <typeparam name="T">The DTO type to map the returned attributes to.</typeparam>
    /// <param name="request">The update request to execute.</param>
    /// <param name="cancellationToken">The cancellation token for the asynchronous operation.</param>
    /// <returns>
    ///     A task that returns the mapped DTO when the request returns attributes; otherwise,
    ///     <see langword="null" />.
    /// </returns>
    public async Task<T?> UpdateItemAsync<T>(
        UpdateItemRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await AmazonDynamoDb.UpdateItemAsync(request, cancellationToken);
        return result.Attributes.Count == 0 ? default : GetMapper<T>().FromItem(result.Attributes);
    }

    /// <summary>Executes a query request and maps each returned item to the specified DTO type.</summary>
    /// <typeparam name="T">The DTO type to map the query results to.</typeparam>
    /// <param name="request">The query request to execute.</param>
    /// <param name="cancellationToken">The cancellation token for the asynchronous operation.</param>
    /// <returns>A task that returns the mapped query results.</returns>
    public async Task<IReadOnlyList<T>> QueryAsync<T>(
        QueryRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await AmazonDynamoDb.QueryAsync(request, cancellationToken);
        var mapper = GetMapper<T>();
        return result.Items.Select(mapper.FromItem).ToArray();
    }

    /// <summary>Executes a scan request and maps each returned item to the specified DTO type.</summary>
    /// <typeparam name="T">The DTO type to map the scan results to.</typeparam>
    /// <param name="request">The scan request to execute.</param>
    /// <param name="cancellationToken">The cancellation token for the asynchronous operation.</param>
    /// <returns>A task that returns the mapped scan results.</returns>
    public async Task<IReadOnlyList<T>> ScanAsync<T>(
        ScanRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await AmazonDynamoDb.ScanAsync(request, cancellationToken);
        var mapper = GetMapper<T>();
        return result.Items.Select(mapper.FromItem).ToArray();
    }
}
