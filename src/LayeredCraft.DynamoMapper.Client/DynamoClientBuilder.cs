using System.Collections.Immutable;
using Amazon.DynamoDBv2;

namespace LayeredCraft.DynamoClient;

public class DynamoClientBuilder
{
    private readonly Dictionary<Type, object> _mappers = new();
    private IAmazonDynamoDB? _dynamoDbClient;

    public DynamoClientBuilder WithMapper<TDto, TMapper>()
        where TMapper : class, IDynamoMapper<TDto>, new()
    {
        _mappers[typeof(TDto)] = new TMapper();
        return this;
    }

    public DynamoClientBuilder WithAmazonDynamoDB(IAmazonDynamoDB dynamoDbClient)
    {
        _dynamoDbClient = dynamoDbClient;
        return this;
    }

    public DynamoClient Build()
    {
        _dynamoDbClient ??= new AmazonDynamoDBClient();

        return new DynamoClient(_mappers.ToImmutableDictionary(), _dynamoDbClient);
    }
}
