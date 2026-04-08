using Amazon.DynamoDBv2.Model;

namespace LayeredCraft.DynamoMapper.IntegrationTests;

internal static class CanonicalModelFactory
{
    internal static CanonicalIntegrationModel CreateModel() => new()
    {
        Id = "entity-001",
        IsEnabled = true,
        Count = 42,
        TotalCount = 9876543210,
        Ratio = 1.25f,
        Score = 12345.6789d,
        Amount = 9876.54m,
        CreatedAt = new DateTime(2025, 01, 02, 03, 04, 05, DateTimeKind.Utc),
        UpdatedAt = new DateTimeOffset(2025, 01, 02, 03, 04, 05, TimeSpan.FromHours(-7)),
        ProcessingTime = TimeSpan.FromMinutes(90) + TimeSpan.FromSeconds(5),
        CorrelationId = Guid.Parse("11111111-2222-3333-4444-555555555555"),
        Status = CanonicalStatus.Active,
        OptionalText = "optional-value",
        OptionalNumber = 123,
        OptionalFlag = false,
        Tags = ["alpha", "beta", "gamma"],
        Scores = [7, 8, 9],
        Aliases = ["first", "second"],
        PriceByMarket = new Dictionary<string, decimal> { ["us"] = 12.34m, ["eu"] = 56.78m },
        Labels = ["new", "sale"],
        ImportanceCodes = [10, 20],
        PayloadVersions = [new byte[] { 0, 1, 2 }, new byte[] { 3, 4, 5 }],
        PayloadByName =
            new Dictionary<string, byte[]>
            {
                ["thumbnail"] = new byte[] { 9, 8, 7 }, ["full"] = new byte[] { 6, 5, 4 },
            },
        UniquePayloads =
            new HashSet<byte[]> { new byte[] { 11, 12, 13 }, new byte[] { 14, 15, 16 } },
        Detail =
            new CanonicalNestedDetail
            {
                Region = "us-west-2",
                ReviewedAt = new DateTimeOffset(2025, 02, 03, 04, 05, 06, TimeSpan.Zero),
                IsPrimary = true,
            },
        Contacts =
        [
            new CanonicalChild
            {
                Name = "Alicia",
                Identifier = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
                LastSeenAt = new DateTime(2025, 03, 04, 05, 06, 07, DateTimeKind.Utc),
            },
            new CanonicalChild
            {
                Name = "Boris",
                Identifier = Guid.Parse("bbbbbbbb-cccc-dddd-eeee-ffffffffffff"),
                LastSeenAt = new DateTime(2025, 04, 05, 06, 07, 08, DateTimeKind.Utc),
            },
        ],
        ContactsById =
            new Dictionary<string, CanonicalChild>
            {
                ["primary"] =
                    new()
                    {
                        Name = "Carla",
                        Identifier = Guid.Parse("cccccccc-dddd-eeee-ffff-111111111111"),
                        LastSeenAt =
                            new DateTime(2025, 05, 06, 07, 08, 09, DateTimeKind.Utc),
                    },
                ["backup"] =
                    new()
                    {
                        Name = "Dylan",
                        Identifier = Guid.Parse("dddddddd-eeee-ffff-1111-222222222222"),
                        LastSeenAt =
                            new DateTime(2025, 06, 07, 08, 09, 10, DateTimeKind.Utc),
                    },
            },
    };

    internal static Dictionary<string, AttributeValue> CreateItem()
    {
        var model = CreateModel();

        return new Dictionary<string, AttributeValue>
        {
            ["id"] = new() { S = model.Id },
            ["isEnabled"] = new() { BOOL = model.IsEnabled },
            ["count"] = new() { N = "42" },
            ["totalCount"] = new() { N = "9876543210" },
            ["ratio"] = new() { N = "1.25" },
            ["score"] = new() { N = "12345.6789" },
            ["amount"] = new() { N = "9876.54" },
            ["createdAt"] = new() { S = "2025-01-02T03:04:05.0000000Z" },
            ["updatedAt"] = new() { S = "2025-01-02T03:04:05.0000000-07:00" },
            ["processingTime"] = new() { S = "01:30:05" },
            ["correlationId"] = new() { S = "11111111-2222-3333-4444-555555555555" },
            ["status"] = new() { S = "Active" },
            ["optionalText"] = new() { S = "optional-value" },
            ["optionalNumber"] = new() { N = "123" },
            ["optionalFlag"] = new() { BOOL = false },
            ["tags"] =
                new()
                {
                    L =
                    [
                        new AttributeValue { S = "alpha" },
                        new AttributeValue { S = "beta" },
                        new AttributeValue { S = "gamma" },
                    ],
                },
            ["scores"] =
                new()
                {
                    L =
                    [
                        new AttributeValue { N = "7" },
                        new AttributeValue { N = "8" },
                        new AttributeValue { N = "9" },
                    ],
                },
            ["aliases"] =
                new()
                {
                    L =
                    [
                        new AttributeValue { S = "first" },
                        new AttributeValue { S = "second" },
                    ],
                },
            ["priceByMarket"] =
                new()
                {
                    M =
                        new Dictionary<string, AttributeValue>
                        {
                            ["us"] = new() { N = "12.34" },
                            ["eu"] = new() { N = "56.78" },
                        },
                },
            ["labels"] = new() { SS = ["new", "sale"] },
            ["importanceCodes"] = new() { NS = ["10", "20"] },
            ["payloadVersions"] =
                new()
                {
                    L =
                    [
                        new AttributeValue
                        {
                            B = new MemoryStream(new byte[] { 0, 1, 2 }),
                        },
                        new AttributeValue
                        {
                            B = new MemoryStream(new byte[] { 3, 4, 5 }),
                        },
                    ],
                },
            ["payloadByName"] =
                new()
                {
                    M =
                        new Dictionary<string, AttributeValue>
                        {
                            ["thumbnail"] =
                                new() { B = new MemoryStream(new byte[] { 9, 8, 7 }) },
                            ["full"] =
                                new() { B = new MemoryStream(new byte[] { 6, 5, 4 }) },
                        },
                },
            ["uniquePayloads"] =
                new()
                {
                    BS =
                    [
                        new MemoryStream(new byte[] { 11, 12, 13 }),
                        new MemoryStream(new byte[] { 14, 15, 16 }),
                    ],
                },
            ["detail"] =
                new()
                {
                    M =
                        new Dictionary<string, AttributeValue>
                        {
                            ["region"] = new() { S = "us-west-2" },
                            ["reviewedAt"] =
                                new() { S = "2025-02-03T04:05:06.0000000+00:00" },
                            ["isPrimary"] = new() { BOOL = true },
                        },
                },
            ["contacts"] =
                new()
                {
                    L =
                    [
                        new AttributeValue
                        {
                            M =
                                new Dictionary<string, AttributeValue>
                                {
                                    ["name"] = new() { S = "Alicia" },
                                    ["identifier"] =
                                        new()
                                        {
                                            S =
                                                "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee",
                                        },
                                    ["lastSeenAt"] =
                                        new()
                                        {
                                            S =
                                                "2025-03-04T05:06:07.0000000Z",
                                        },
                                },
                        },
                        new AttributeValue
                        {
                            M =
                                new Dictionary<string, AttributeValue>
                                {
                                    ["name"] = new() { S = "Boris" },
                                    ["identifier"] =
                                        new()
                                        {
                                            S =
                                                "bbbbbbbb-cccc-dddd-eeee-ffffffffffff",
                                        },
                                    ["lastSeenAt"] =
                                        new()
                                        {
                                            S =
                                                "2025-04-05T06:07:08.0000000Z",
                                        },
                                },
                        },
                    ],
                },
            ["contactsById"] =
                new()
                {
                    M =
                        new Dictionary<string, AttributeValue>
                        {
                            ["primary"] =
                                new()
                                {
                                    M =
                                        new Dictionary<string, AttributeValue>
                                        {
                                            ["name"] = new() { S = "Carla" },
                                            ["identifier"] =
                                                new()
                                                {
                                                    S =
                                                        "cccccccc-dddd-eeee-ffff-111111111111",
                                                },
                                            ["lastSeenAt"] =
                                                new()
                                                {
                                                    S =
                                                        "2025-05-06T07:08:09.0000000Z",
                                                },
                                        },
                                },
                            ["backup"] =
                                new()
                                {
                                    M =
                                        new Dictionary<string, AttributeValue>
                                        {
                                            ["name"] = new() { S = "Dylan" },
                                            ["identifier"] =
                                                new()
                                                {
                                                    S =
                                                        "dddddddd-eeee-ffff-1111-222222222222",
                                                },
                                            ["lastSeenAt"] =
                                                new()
                                                {
                                                    S =
                                                        "2025-06-07T08:09:10.0000000Z",
                                                },
                                        },
                                },
                        },
                },
        };
    }
}
