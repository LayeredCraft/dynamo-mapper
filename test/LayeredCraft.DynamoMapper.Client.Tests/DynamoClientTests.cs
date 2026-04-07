using Amazon.DynamoDBv2.Model;
using LayeredCraft.DynamoMapper.Client.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace LayeredCraft.DynamoMapper.Client.Tests;

public sealed class DynamoClientTests(DynamoDbFixture fixture) : IClassFixture<DynamoDbFixture>
{
    private readonly DynamoClient _client = new DynamoClientBuilder()
        .WithAmazonDynamoDb(fixture.Client)
        .WithMapper<UserProfile, UserProfileMapper>()
        .WithMapper<ProjectRecord, ProjectRecordMapper>()
        .WithMapper<TaskRecord, TaskRecordMapper>()
        .Build();

    [Fact]
    public async Task GetItemAsync_UserProfile_ReturnsSeededItem()
    {
        var expected = TestDataSamples.UserProfiles[0];

        var item = await _client.GetItemAsync<UserProfile>(
            DynamoDbFixture.TableName,
            CreateKey(expected.Pk, expected.Sk),
            TestContext.Current.CancellationToken);

        item.MappedItem.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task QueryAsync_ProjectRecord_ReturnsSeededProjectsForOwner()
    {
        var expected = TestDataSamples.ProjectRecords[0];

        var items = await _client.QueryAsync<ProjectRecord>(
            new QueryRequest
            {
                TableName = DynamoDbFixture.TableName,
                KeyConditionExpression = "pk = :pk AND begins_with(sk, :skPrefix)",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    [":pk"] = new() { S = expected.Pk },
                    [":skPrefix"] = new() { S = "PROJECT#" },
                },
            },
            TestContext.Current.CancellationToken);

        items.Should().ContainEquivalentOf(expected);
    }

    [Fact]
    public async Task ScanAsync_UserProfile_ReturnsSeededProfiles()
    {
        var items = await _client.ScanAsync<UserProfile>(
            new ScanRequest
            {
                TableName = DynamoDbFixture.TableName,
                FilterExpression = "entityType = :entityType",
                ExpressionAttributeValues =
                    new Dictionary<string, AttributeValue>
                    {
                        [":entityType"] = new() { S = "UserProfile" },
                    },
            },
            TestContext.Current.CancellationToken);

        items.Should().BeEquivalentTo(TestDataSamples.UserProfiles);
    }

    [Fact]
    public async Task ExecuteStatementAsync_UserProfile_ReturnsSeededProfiles()
    {
        var items = await _client.ExecuteStatementAsync<UserProfile>(
            new ExecuteStatementRequest
            {
                Statement = $"""
                             SELECT * FROM "{DynamoDbFixture.TableName}"
                             WHERE entityType = ?
                             """,
                Parameters = [new AttributeValue { S = "UserProfile" }],
            },
            TestContext.Current.CancellationToken);

        items.Should().BeEquivalentTo(TestDataSamples.UserProfiles);
    }

    [Fact]
    public async Task ExecuteStatementAsync_RawResponse_ReturnsDynamoDbItems()
    {
        var response = await _client.ExecuteStatementAsync(
            new ExecuteStatementRequest
            {
                Statement =
                    $"SELECT * FROM \"{DynamoDbFixture.TableName}\" WHERE pk = ? AND sk = ?",
                Parameters =
                [
                    new AttributeValue { S = TestDataSamples.UserProfiles[0].Pk },
                    new AttributeValue { S = TestDataSamples.UserProfiles[0].Sk },
                ],
            },
            TestContext.Current.CancellationToken);

        response.Items.Should().ContainSingle();
        response.Items[0]["entityType"].S.Should().Be("UserProfile");
    }

    [Fact]
    public async Task PutItemAsync_ThenDeleteItemAsync_PersistsAndRemovesItem()
    {
        var item = new TaskRecord
        {
            Pk = "PROJECT#p-9999",
            Sk = "TASK#t-9999",
            EntityType = "TaskRecord",
            TaSkId = "t-9999",
            ProjectId = "p-9999",
            AssignedUserId = "u-1001",
            Title = "Verify client put",
            Notes = "Inserted by integration test.",
            EstimateHours = 2.5m,
            Completed = false,
            Order = 99,
            CreatedAt = "2025-04-06T10:00:00Z",
            DueAt = "2025-04-07T10:00:00Z",
            Checklist =
            [
                new TaSkChecklistItem { Text = "Write item", Done = true },
                new TaSkChecklistItem { Text = "Read item", Done = false },
            ],
            Metadata = new TaSkMetadata { Color = "blue", BlockedBy = null },
        };

        await _client.PutItemAsync(
            DynamoDbFixture.TableName,
            item,
            TestContext.Current.CancellationToken);

        var persisted = await _client.GetItemAsync<TaskRecord>(
            DynamoDbFixture.TableName,
            CreateKey(item.Pk, item.Sk),
            TestContext.Current.CancellationToken);

        persisted.Should().BeEquivalentTo(item);

        await _client.DeleteItemAsync(
            DynamoDbFixture.TableName,
            CreateKey(item.Pk, item.Sk),
            TestContext.Current.CancellationToken);

        var deleted = await _client.GetItemAsync<TaskRecord>(
            DynamoDbFixture.TableName,
            CreateKey(item.Pk, item.Sk),
            TestContext.Current.CancellationToken);

        deleted.MappedItem.Should().BeNull();
    }

    [Fact]
    public async Task UpdateItemAsync_TaskRecord_ReturnsMappedUpdatedItem()
    {
        var existing = TestDataSamples.TaskRecords[0];
        var expected = new TaskRecord
        {
            Pk = existing.Pk,
            Sk = existing.Sk,
            EntityType = existing.EntityType,
            TaSkId = existing.TaSkId,
            ProjectId = existing.ProjectId,
            AssignedUserId = existing.AssignedUserId,
            Title = existing.Title,
            Notes = "Updated by integration test.",
            EstimateHours = existing.EstimateHours,
            Completed = false,
            Order = existing.Order,
            CreatedAt = existing.CreatedAt,
            DueAt = existing.DueAt,
            Checklist = existing.Checklist,
            Metadata = existing.Metadata,
        };

        var updated = await _client.UpdateItemAsync<TaskRecord>(
            new UpdateItemRequest
            {
                TableName = DynamoDbFixture.TableName,
                Key = CreateKey(existing.Pk, existing.Sk),
                UpdateExpression = "SET notes = :notes, completed = :completed",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    [":notes"] = new() { S = "Updated by integration test." },
                    [":completed"] = new() { BOOL = false },
                },
                ReturnValues = "ALL_NEW",
            },
            TestContext.Current.CancellationToken);

        updated.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task AddDynamoClient_ResolvesWorkingClient()
    {
        var services = new ServiceCollection();
        services.AddSingleton(fixture.Client);

        services.AddDynamoClient(builder =>
        {
            builder.AddMapper<UserProfile, UserProfileMapper>();
            builder.AddMapper<ProjectRecord, ProjectRecordMapper>();
            builder.AddMapper<TaskRecord, TaskRecordMapper>();
        });

        await using var serviceProvider = services.BuildServiceProvider();
        var client = serviceProvider.GetRequiredService<DynamoClient>();
        var expected = TestDataSamples.UserProfiles[1];

        var item = await client.GetItemAsync<UserProfile>(
            DynamoDbFixture.TableName,
            CreateKey(expected.Pk, expected.Sk),
            TestContext.Current.CancellationToken);

        item.MappedItem.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task AddDynamoClient_WithAmazonDynamoDbOverride_ResolvesWorkingClient()
    {
        var services = new ServiceCollection();

        services.AddDynamoClient(builder =>
        {
            builder.WithAmazonDynamoDb(fixture.Client);
            builder.AddMapper<UserProfile, UserProfileMapper>();
            builder.AddMapper<ProjectRecord, ProjectRecordMapper>();
            builder.AddMapper<TaskRecord, TaskRecordMapper>();
        });

        await using var serviceProvider = services.BuildServiceProvider();
        var client = serviceProvider.GetRequiredService<DynamoClient>();
        var expected = TestDataSamples.ProjectRecords[1];

        var items = await client.QueryAsync<ProjectRecord>(
            new QueryRequest
            {
                TableName = DynamoDbFixture.TableName,
                KeyConditionExpression = "pk = :pk AND begins_with(sk, :skPrefix)",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    [":pk"] = new() { S = expected.Pk },
                    [":skPrefix"] = new() { S = "PROJECT#" },
                },
            },
            TestContext.Current.CancellationToken);

        items.Should().ContainEquivalentOf(expected);
    }

    private static Dictionary<string, AttributeValue> CreateKey(string pk, string sk)
        => new() { ["pk"] = new AttributeValue { S = pk }, ["sk"] = new AttributeValue { S = sk } };
}
