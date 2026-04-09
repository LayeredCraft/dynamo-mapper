using Amazon.DynamoDBv2.Model;

namespace LayeredCraft.DynamoMapper.IntegrationTests;

internal static class CanonicalModelAssertions
{
    internal static void AssertEquivalent(
        CanonicalIntegrationModel expected, CanonicalIntegrationModel actual
    )
    {
        actual.Id.Should().Be(expected.Id);
        actual.IsEnabled.Should().Be(expected.IsEnabled);
        actual.Count.Should().Be(expected.Count);
        actual.TotalCount.Should().Be(expected.TotalCount);
        actual.Ratio.Should().Be(expected.Ratio);
        actual.Score.Should().Be(expected.Score);
        actual.Amount.Should().Be(expected.Amount);
        actual.CreatedAt.Should().Be(expected.CreatedAt);
        actual.UpdatedAt.Should().Be(expected.UpdatedAt);
        actual.ProcessingTime.Should().Be(expected.ProcessingTime);
        actual.CorrelationId.Should().Be(expected.CorrelationId);
        actual.Status.Should().Be(expected.Status);
        actual.OptionalText.Should().Be(expected.OptionalText);
        actual.OptionalNumber.Should().Be(expected.OptionalNumber);
        actual.OptionalFlag.Should().Be(expected.OptionalFlag);

        actual.Tags.Should().Equal(expected.Tags);
        actual.Scores.Should().Equal(expected.Scores);
        actual.Aliases.Should().Equal(expected.Aliases);
        actual.PriceByMarket.Should().Equal(expected.PriceByMarket);
        actual.Labels.Should().BeEquivalentTo(expected.Labels);
        actual.ImportanceCodes.Should().BeEquivalentTo(expected.ImportanceCodes);

        actual.PayloadVersions.Select(ToBase64)
            .Should()
            .Equal(expected.PayloadVersions.Select(ToBase64));
        actual.PayloadByName.Should().HaveCount(expected.PayloadByName.Count);
        actual.PayloadByName.Keys.Should().BeEquivalentTo(expected.PayloadByName.Keys);

        foreach (var pair in expected.PayloadByName)
            actual.PayloadByName[pair.Key].Should().Equal(pair.Value);

        actual.UniquePayloads.Select(ToBase64)
            .Should()
            .BeEquivalentTo(expected.UniquePayloads.Select(ToBase64));

        actual.Detail.Region.Should().Be(expected.Detail.Region);
        actual.Detail.ReviewedAt.Should().Be(expected.Detail.ReviewedAt);
        actual.Detail.IsPrimary.Should().Be(expected.Detail.IsPrimary);

        actual.Contacts.Should().HaveCount(expected.Contacts.Count);
        for (var i = 0; i < expected.Contacts.Count; i++)
            AssertChildEquivalent(expected.Contacts[i], actual.Contacts[i]);

        actual.ContactsById.Keys.Should().BeEquivalentTo(expected.ContactsById.Keys);
        foreach (var pair in expected.ContactsById)
            AssertChildEquivalent(pair.Value, actual.ContactsById[pair.Key]);
    }

    internal static void AssertExpectedItemShape(Dictionary<string, AttributeValue> item)
    {
        item.Keys.Should()
            .BeEquivalentTo(
                "id",
                "isEnabled",
                "count",
                "totalCount",
                "ratio",
                "score",
                "amount",
                "createdAt",
                "updatedAt",
                "processingTime",
                "correlationId",
                "status",
                "optionalText",
                "optionalNumber",
                "optionalFlag",
                "tags",
                "scores",
                "aliases",
                "priceByMarket",
                "labels",
                "importanceCodes",
                "payloadVersions",
                "payloadByName",
                "uniquePayloads",
                "detail",
                "contacts",
                "contactsById"
            );

        item["id"].S.Should().Be("entity-001");
        item["isEnabled"].BOOL.Should().BeTrue();
        item["count"].N.Should().Be("42");
        item["totalCount"].N.Should().Be("9876543210");
        item["ratio"].N.Should().Be("1.25");
        item["score"].N.Should().Be("12345.6789");
        item["amount"].N.Should().Be("9876.54");
        item["createdAt"].S.Should().Be("2025-01-02T03:04:05.0000000Z");
        item["updatedAt"].S.Should().Be("2025-01-02T03:04:05.0000000-07:00");
        item["processingTime"].S.Should().Be("01:30:05");
        item["correlationId"].S.Should().Be("11111111-2222-3333-4444-555555555555");
        item["status"].S.Should().Be("Active");
        item["optionalText"].S.Should().Be("optional-value");
        item["optionalNumber"].N.Should().Be("123");
        item["optionalFlag"].BOOL.Should().BeFalse();

        item["tags"].L.Select(value => value.S).Should().Equal("alpha", "beta", "gamma");
        item["scores"].L.Select(value => value.N).Should().Equal("7", "8", "9");
        item["aliases"].L.Select(value => value.S).Should().Equal("first", "second");

        item["priceByMarket"].M["us"].N.Should().Be("12.34");
        item["priceByMarket"].M["eu"].N.Should().Be("56.78");

        item["labels"].SS.Should().BeEquivalentTo("new", "sale");
        item["importanceCodes"].NS.Should().BeEquivalentTo("10", "20");

        item["payloadVersions"].L.Should().HaveCount(2);
        AssertBinary(item["payloadVersions"].L[0], new byte[] { 0, 1, 2 });
        AssertBinary(item["payloadVersions"].L[1], new byte[] { 3, 4, 5 });

        item["payloadByName"].M.Keys.Should().BeEquivalentTo("thumbnail", "full");
        AssertBinary(item["payloadByName"].M["thumbnail"], new byte[] { 9, 8, 7 });
        AssertBinary(item["payloadByName"].M["full"], new byte[] { 6, 5, 4 });

        item["uniquePayloads"].BS.Should().HaveCount(2);
        item["uniquePayloads"]
            .BS.Select(ToBase64)
            .Should()
            .BeEquivalentTo(
                Convert.ToBase64String(new byte[] { 11, 12, 13 }),
                Convert.ToBase64String(new byte[] { 14, 15, 16 })
            );

        item["detail"].M["region"].S.Should().Be("us-west-2");
        item["detail"].M["reviewedAt"].S.Should().Be("2025-02-03T04:05:06.0000000+00:00");
        item["detail"].M["isPrimary"].BOOL.Should().BeTrue();

        item["contacts"].L.Should().HaveCount(2);
        item["contacts"].L[0].M["name"].S.Should().Be("Alicia");
        item["contacts"].L[0].M["identifier"].S.Should().Be("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
        item["contacts"].L[0].M["lastSeenAt"].S.Should().Be("2025-03-04T05:06:07.0000000Z");
        item["contacts"].L[1].M["name"].S.Should().Be("Boris");
        item["contacts"].L[1].M["identifier"].S.Should().Be("bbbbbbbb-cccc-dddd-eeee-ffffffffffff");
        item["contacts"].L[1].M["lastSeenAt"].S.Should().Be("2025-04-05T06:07:08.0000000Z");

        item["contactsById"].M.Keys.Should().BeEquivalentTo("primary", "backup");
        item["contactsById"].M["primary"].M["name"].S.Should().Be("Carla");
        item["contactsById"].M["backup"].M["name"].S.Should().Be("Dylan");
    }

    private static void AssertChildEquivalent(CanonicalChild expected, CanonicalChild actual)
    {
        actual.Name.Should().Be(expected.Name);
        actual.Identifier.Should().Be(expected.Identifier);
        actual.LastSeenAt.Should().Be(expected.LastSeenAt);
    }

    private static void AssertBinary(AttributeValue value, byte[] expected)
    {
        value.B.Should().NotBeNull();
        value.B.Should().BeOfType<MemoryStream>();
        value.B!.ToArray().Should().Equal(expected);
    }

    private static string ToBase64(byte[] bytes) => Convert.ToBase64String(bytes);

    private static string ToBase64(MemoryStream stream) => Convert.ToBase64String(stream.ToArray());
}
