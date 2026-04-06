using Amazon.DynamoDBv2.Model;

namespace LayeredCraft.DynamoMapper.Client.Tests;

public sealed class DynamoClientTests(DynamoDbFixture fixture) : IClassFixture<DynamoDbFixture>
{
    private readonly DynamoClient _client = new DynamoClientBuilder()
        .WithAmazonDynamoDB(fixture.Client)
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

        item.Should().BeEquivalentTo(expected);
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

        deleted.Should().BeNull();
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

    private static Dictionary<string, AttributeValue> CreateKey(string pk, string sk)
        => new() { ["pk"] = new AttributeValue { S = pk }, ["sk"] = new AttributeValue { S = sk } };
}
