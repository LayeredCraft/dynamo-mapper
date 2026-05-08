using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Testcontainers.DynamoDb;

namespace LayeredCraft.DynamoMapper.Client.Tests;

public sealed class DynamoDbFixture : IAsyncLifetime
{
    private static readonly UserProfileMapper UserProfiles = new();
    private static readonly ProjectRecordMapper ProjectRecords = new();
    private static readonly TaskRecordMapper TaskRecords = new();

    public const string TableName = "test-data";

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

    public async ValueTask InitializeAsync()
    {
        await Container.StartAsync(CancellationToken);

        await Client.CreateTableAsync(
            new CreateTableRequest
            {
                TableName = TableName,
                BillingMode = BillingMode.PAY_PER_REQUEST,
                AttributeDefinitions =
                [
                    new AttributeDefinition("pk", ScalarAttributeType.S),
                    new AttributeDefinition("sk", ScalarAttributeType.S),
                ],
                KeySchema =
                [
                    new KeySchemaElement("pk", KeyType.HASH),
                    new KeySchemaElement("sk", KeyType.RANGE),
                ],
            },
            CancellationToken);

        var writeRequests =
            TestDataSamples
                .UserProfiles
                .Select(UserProfiles.ToItem)
                .Concat(TestDataSamples.ProjectRecords.Select(ProjectRecords.ToItem))
                .Concat(TestDataSamples.TaskRecords.Select(TaskRecords.ToItem))
                .Select(item => new WriteRequest { PutRequest = new PutRequest { Item = item } })
                .ToArray();

        foreach (var batch in writeRequests.Chunk(25))
            await WriteBatchUntilCompleteAsync(batch);
    }

    private async Task WriteBatchUntilCompleteAsync(IReadOnlyCollection<WriteRequest> batch)
    {
        var pending = batch.ToArray();

        while (pending.Length > 0)
        {
            var response = await Client.BatchWriteItemAsync(
                new BatchWriteItemRequest
                {
                    RequestItems =
                        new Dictionary<string, List<WriteRequest>>
                        {
                            [TableName] = pending.ToList(),
                        },
                },
                CancellationToken);

            pending = response.UnprocessedItems.TryGetValue(TableName, out var unprocessed)
                ? unprocessed.ToArray()
                : [];
        }
    }
}
