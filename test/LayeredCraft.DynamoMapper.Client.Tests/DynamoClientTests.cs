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

        var response = await _client.QueryAsync<ProjectRecord>(
            new QueryRequest
            {
                TableName = DynamoDbFixture.TableName,
                KeyConditionExpression = "pk = :pk AND begins_with(sk, :skPrefix)",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    [":pk"] = expected.Pk.ToAttributeValue(),
                    [":skPrefix"] = "PROJECT#".ToAttributeValue(),
                },
            },
            TestContext.Current.CancellationToken);

        response.MappedItems.Should().ContainEquivalentOf(expected);
    }

    [Fact]
    public async Task ScanAsync_UserProfile_ReturnsSeededProfiles()
    {
        var response = await _client.ScanAsync<UserProfile>(
            new ScanRequest
            {
                TableName = DynamoDbFixture.TableName,
                FilterExpression = "entityType = :entityType",
                ExpressionAttributeValues =
                    new Dictionary<string, AttributeValue>
                    {
                        [":entityType"] = "UserProfile".ToAttributeValue(),
                    },
            },
            TestContext.Current.CancellationToken);

        response.MappedItems.Should().BeEquivalentTo(TestDataSamples.UserProfiles);
    }

    [Fact]
    public async Task ExecuteStatementAsync_UserProfile_ReturnsSeededProfiles()
    {
        var response = await _client.ExecuteStatementAsync<UserProfile>(
            new ExecuteStatementRequest
            {
                Statement = $"""
                             SELECT * FROM "{DynamoDbFixture.TableName}"
                             WHERE entityType = ?
                             """,
                Parameters = ["UserProfile".ToAttributeValue()],
            },
            TestContext.Current.CancellationToken);

        response.MappedItems.Should().BeEquivalentTo(TestDataSamples.UserProfiles);
    }

    [Fact]
    public async Task ExecuteStatementAsync_UserProfile_WithKeyFilter_ReturnsMappedItem()
    {
        var expected = TestDataSamples.UserProfiles[0];

        var response = await _client.ExecuteStatementAsync<UserProfile>(
            new ExecuteStatementRequest
            {
                Statement = $"""
                             SELECT * FROM "{DynamoDbFixture.TableName}"
                             WHERE pk = ? AND sk = ?
                             """,
                Parameters = [expected.Pk.ToAttributeValue(), expected.Sk.ToAttributeValue()],
            },
            TestContext.Current.CancellationToken);

        response.MappedItems.Should().BeEquivalentTo([expected]);
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

        persisted.MappedItem.Should().BeEquivalentTo(item);

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
    public async Task PutItemAsync_UserProfile_WithAllOld_ReturnsMappedOldItem()
    {
        var original = new UserProfile
        {
            Pk = "USER#u-9998",
            Sk = "PROFILE#u-9998",
            EntityType = "UserProfile",
            UserId = "u-9998",
            Email = "original@example.com",
            DisplayName = "Original User",
            Age = 41,
            IsActive = true,
            AccountBalance = 10.25m,
            CreatedAt = "2025-04-01T00:00:00Z",
            LastLoginEpoch = 1743465600,
            Tags = ["temp", "original"],
            Preferences =
                new UserPreferences
                {
                    Theme = "dark", NotificationsEnabled = true, Language = "en-US",
                },
            LoginHistory =
            [
                new LoginHistoryEntry
                {
                    At = "2025-04-01T00:00:00Z", IpAddress = "203.0.113.99",
                },
            ],
            ProfilePhoto = [9, 9, 9],
        };
        var replacement = new UserProfile
        {
            Pk = original.Pk,
            Sk = original.Sk,
            EntityType = original.EntityType,
            UserId = original.UserId,
            Email = "updated@example.com",
            DisplayName = "Updated User",
            Age = original.Age,
            IsActive = original.IsActive,
            AccountBalance = original.AccountBalance,
            CreatedAt = original.CreatedAt,
            LastLoginEpoch = original.LastLoginEpoch,
            Tags = original.Tags,
            Preferences = original.Preferences,
            LoginHistory = original.LoginHistory,
            ProfilePhoto = original.ProfilePhoto,
        };

        await _client.PutItemAsync(
            DynamoDbFixture.TableName,
            original,
            TestContext.Current.CancellationToken);

        var response = await _client.PutItemAsync<UserProfile>(
            new PutItemRequest
            {
                TableName = DynamoDbFixture.TableName,
                Item = _client.GetMapper<UserProfile>().ToItem(replacement),
                ReturnValues = "ALL_OLD",
            },
            TestContext.Current.CancellationToken);

        response.MappedItem.Should().BeEquivalentTo(original);

        await _client.DeleteItemAsync(
            DynamoDbFixture.TableName,
            CreateKey(original.Pk, original.Sk),
            TestContext.Current.CancellationToken);
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
                    [":notes"] = "Updated by integration test.".ToAttributeValue(),
                    [":completed"] = false.ToAttributeValue(),
                },
                ReturnValues = "ALL_NEW",
            },
            TestContext.Current.CancellationToken);

        updated.MappedItem.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task DeleteItemAsync_TaskRecord_WithAllOld_ReturnsMappedDeletedItem()
    {
        var existing = new TaskRecord
        {
            Pk = "PROJECT#p-9998",
            Sk = "TASK#t-9998",
            EntityType = "TaskRecord",
            TaSkId = "t-9998",
            ProjectId = "p-9998",
            AssignedUserId = "u-1001",
            Title = "Temporary task",
            Notes = "Delete me",
            EstimateHours = 1.5m,
            Completed = false,
            Order = 1,
            CreatedAt = "2025-04-01T00:00:00Z",
            DueAt = "2025-04-02T00:00:00Z",
            Checklist = [new TaSkChecklistItem { Text = "One", Done = false }],
            Metadata = new TaSkMetadata { Color = "green", BlockedBy = null },
        };

        await _client.PutItemAsync(
            DynamoDbFixture.TableName,
            existing,
            TestContext.Current.CancellationToken);

        var deleted = await _client.DeleteItemAsync<TaskRecord>(
            new DeleteItemRequest
            {
                TableName = DynamoDbFixture.TableName,
                Key = CreateKey(existing.Pk, existing.Sk),
                ReturnValues = "ALL_OLD",
            },
            TestContext.Current.CancellationToken);

        deleted.MappedItem.Should().BeEquivalentTo(existing);
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

        var response = await client.QueryAsync<ProjectRecord>(
            new QueryRequest
            {
                TableName = DynamoDbFixture.TableName,
                KeyConditionExpression = "pk = :pk AND begins_with(sk, :skPrefix)",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    [":pk"] = expected.Pk.ToAttributeValue(),
                    [":skPrefix"] = "PROJECT#".ToAttributeValue(),
                },
            },
            TestContext.Current.CancellationToken);

        response.MappedItems.Should().ContainEquivalentOf(expected);
    }

    private static Dictionary<string, AttributeValue> CreateKey(string pk, string sk)
        => new() { ["pk"] = pk.ToAttributeValue(), ["sk"] = sk.ToAttributeValue() };
}
