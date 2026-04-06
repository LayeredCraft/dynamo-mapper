using System.Collections.Immutable;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace LayeredCraft.DynamoClient;

public class DynamoClient
{
    private readonly ImmutableDictionary<Type, object> _mappers;
    private readonly IAmazonDynamoDB _dynamoDbClient;

    internal DynamoClient(ImmutableDictionary<Type, object> mappers, IAmazonDynamoDB dynamoDbClient)
    {
        ArgumentNullException.ThrowIfNull(dynamoDbClient);

        _mappers = mappers;
        _dynamoDbClient = dynamoDbClient;
    }

    public IDynamoMapper<T> GetMapper<T>()
    {
        if (_mappers.TryGetValue(typeof(T), out var mapper))
            return (IDynamoMapper<T>)mapper;

        throw new InvalidOperationException($"No mapper found for type {typeof(T)}");
    }

    public T? GetItemAsync<T>(
        string tableName,
        Dictionary<string, AttributeValue> key,
        CancellationToken cancellationToken = default)
    {
        var result = _dynamoDbClient.GetItemAsync(tableName, key, cancellationToken);
        return result is null ? default : GetMapper<T>().FromItem(result.Result.Item);
    }
}
