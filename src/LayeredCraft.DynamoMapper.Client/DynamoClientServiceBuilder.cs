using System.Diagnostics.CodeAnalysis;
using Amazon.DynamoDBv2;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LayeredCraft.DynamoMapper.Client;

/// <summary>
///     Provides a fluent registration surface for configuring <see cref="DynamoClient" /> in a
///     dependency injection container.
/// </summary>
public sealed class DynamoClientServiceBuilder
{
    private readonly IList<(Type DtoType, Type MapperType)> _registrations;

    internal DynamoClientServiceBuilder(
        IServiceCollection services,
        IList<(Type DtoType, Type MapperType)> registrations)
    {
        ArgumentNullException.ThrowIfNull(services);

        Services = services;
        _registrations = registrations;
    }

    /// <summary>Gets the service collection being configured.</summary>
    public IServiceCollection Services { get; }

    internal IAmazonDynamoDB? AmazonDynamoDb { get; private set; }

    /// <summary>Uses the specified DynamoDB client when building the <see cref="DynamoClient" />.</summary>
    /// <param name="dynamoDbClient">The DynamoDB client instance to use.</param>
    /// <returns>The current registration builder.</returns>
    public DynamoClientServiceBuilder WithAmazonDynamoDb(IAmazonDynamoDB dynamoDbClient)
    {
        ArgumentNullException.ThrowIfNull(dynamoDbClient);

        AmazonDynamoDb = dynamoDbClient;
        return this;
    }

    /// <summary>Registers a mapper that should be included in the built <see cref="DynamoClient" />.</summary>
    /// <typeparam name="TDto">The DTO type handled by the mapper.</typeparam>
    /// <typeparam name="TMapper">The mapper implementation type.</typeparam>
    /// <returns>The current registration builder.</returns>
    public DynamoClientServiceBuilder AddMapper<TDto,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TMapper>()
        where TMapper : class, IDynamoMapper<TDto>
    {
        Services.TryAddSingleton<TMapper>();
        _registrations.Add((typeof(TDto), typeof(TMapper)));
        return this;
    }
}
