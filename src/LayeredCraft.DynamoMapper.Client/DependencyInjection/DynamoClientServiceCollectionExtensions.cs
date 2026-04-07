using Amazon.DynamoDBv2;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LayeredCraft.DynamoMapper.Client.DependencyInjection;

/// <summary>Provides dependency injection registration helpers for <see cref="DynamoClient" />.</summary>
public static class DynamoClientServiceCollectionExtensions
{
    /// <summary>
    ///     Registers <see cref="DynamoClient" /> as a singleton and applies mapper configuration
    ///     through the returned builder.
    /// </summary>
    /// <param name="services">The service collection to update.</param>
    /// <param name="configure">Applies mapper registrations to the builder.</param>
    /// <returns>The service collection for further chaining.</returns>
    public static IServiceCollection AddDynamoClient(
        this IServiceCollection services,
        Action<DynamoClientServiceBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        var registrations = new List<(Type DtoType, Type MapperType)>();
        var builder = new DynamoClientServiceBuilder(services, registrations);
        configure(builder);

        services.TryAddSingleton<DynamoClient>(serviceProvider =>
        {
            var dynamoDbClient =
                builder.AmazonDynamoDb ?? serviceProvider.GetRequiredService<IAmazonDynamoDB>();
            var clientBuilder = new DynamoClientBuilder().WithAmazonDynamoDb(dynamoDbClient);

            foreach (var registration in registrations)
            {
                var mapper = serviceProvider.GetRequiredService(registration.MapperType);
                clientBuilder.WithMapper(registration.DtoType, mapper);
            }

            return clientBuilder.Build();
        });
        return services;
    }
}
