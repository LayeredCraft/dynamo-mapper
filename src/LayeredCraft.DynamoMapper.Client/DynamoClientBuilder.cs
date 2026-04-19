using System.Collections.Immutable;
using Amazon.DynamoDBv2;

namespace LayeredCraft.DynamoMapper.Client;

/// <summary>Builds a <see cref="DynamoClient" /> with registered mappers and a DynamoDB client.</summary>
public class DynamoClientBuilder
{
    private readonly Dictionary<Type, object> _mappers = new();
    private IAmazonDynamoDB? _dynamoDbClient;

    /// <summary>Registers the specified mapper instance for the DTO type.</summary>
    /// <typeparam name="TDto">The DTO type handled by the mapper.</typeparam>
    /// <param name="mapper">The mapper instance to register.</param>
    /// <returns>The current builder instance.</returns>
    public DynamoClientBuilder WithMapper<TDto>(IDynamoMapper<TDto> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);

        _mappers[typeof(TDto)] = mapper;
        return this;
    }

    /// <summary>Registers a mapper for the specified DTO type.</summary>
    /// <typeparam name="TDto">The DTO type handled by the mapper.</typeparam>
    /// <typeparam name="TMapper">The mapper type to instantiate and register.</typeparam>
    /// <returns>The current builder instance.</returns>
    public DynamoClientBuilder WithMapper<TDto, TMapper>()
        where TMapper : class, IDynamoMapper<TDto>, new()
        => WithMapper(new TMapper());

    /// <summary>Uses the specified DynamoDB client when building the <see cref="DynamoClient" />.</summary>
    /// <param name="dynamoDbClient">The DynamoDB client instance to use.</param>
    /// <returns>The current builder instance.</returns>
    public DynamoClientBuilder WithAmazonDynamoDb(IAmazonDynamoDB dynamoDbClient)
    {
        _dynamoDbClient = dynamoDbClient;
        return this;
    }

    /// <summary>Builds a <see cref="DynamoClient" /> from the configured mappers and DynamoDB client.</summary>
    /// <returns>A configured <see cref="DynamoClient" /> instance.</returns>
    public DynamoClient Build()
    {
        _dynamoDbClient ??= new AmazonDynamoDBClient();

        return new DynamoClient(_mappers.ToImmutableDictionary(), _dynamoDbClient);
    }

    internal DynamoClientBuilder WithMapper(Type dtoType, object mapper)
    {
        ArgumentNullException.ThrowIfNull(dtoType);
        ArgumentNullException.ThrowIfNull(mapper);

        _mappers[dtoType] = mapper;
        return this;
    }
}
