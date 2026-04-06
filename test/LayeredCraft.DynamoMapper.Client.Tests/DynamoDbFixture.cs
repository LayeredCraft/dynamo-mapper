using Amazon.DynamoDBv2;
using Testcontainers.DynamoDb;

namespace LayeredCraft.DynamoMapper.Client.Tests;

public sealed class DynamoDbFixture : IAsyncLifetime
{
    public readonly DynamoDbContainer Container =
        new DynamoDbBuilder("amazon/dynamodb-local:latest").Build();

    public static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    public IAmazonDynamoDB Client
    {
        get
        {
            field ??= new AmazonDynamoDBClient(
                new AmazonDynamoDBConfig { ServiceURL = Container.GetConnectionString() });
            return field;
        }
    }

    public async ValueTask DisposeAsync() => await Container.StopAsync(CancellationToken);

    public async ValueTask InitializeAsync() => await Container.StartAsync(CancellationToken);
}
