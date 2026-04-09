using System.Globalization;
using Amazon.DynamoDBv2.Model;

namespace LayeredCraft.DynamoMapper.IntegrationTests;

public class CanonicalMapperTests
{
    [Fact]
    public void Canonical_ToItem_StoresExpectedAttributeShapes()
    {
        var model = CanonicalModelFactory.CreateModel();

        var item = CanonicalIntegrationModelMapper.ToItem(model);

        CanonicalModelAssertions.AssertExpectedItemShape(item);
    }

    [Fact]
    public void Canonical_FromItem_ReconstructsExpectedModel()
    {
        var expected = CanonicalModelFactory.CreateModel();
        var item = CanonicalModelFactory.CreateItem();

        var actual = CanonicalIntegrationModelMapper.FromItem(item);

        CanonicalModelAssertions.AssertEquivalent(expected, actual);
    }

    [Fact]
    public void Canonical_RoundTrip_PreservesValues()
    {
        var expected = CanonicalModelFactory.CreateModel();

        var item = CanonicalIntegrationModelMapper.ToItem(expected);
        var actual = CanonicalIntegrationModelMapper.FromItem(item);

        CanonicalModelAssertions.AssertEquivalent(expected, actual);
    }

    [Fact]
    public void Canonical_RoundTrip_PreservesGuidCollectionShapes()
    {
        var expected = CanonicalModelFactory.CreateModel();

        var item = CanonicalIntegrationModelMapper.ToItem(expected);
        var actual = CanonicalIntegrationModelMapper.FromItem(item);

        item["relatedIds"]
            .L.Select(value => value.S)
            .Should()
            .Equal(expected.RelatedIds.Select(value => value.ToString("D")));
        item["legacyIds"]
            .L.Select(value => value.S)
            .Should()
            .Equal(expected.LegacyIds.Select(value => value.ToString("D")));
        item["alternateIds"]
            .L.Select(value => value.S)
            .Should()
            .Equal(expected.AlternateIds.Select(value => value.ToString("D")));
        item["uniqueIds"]
            .L.Select(value => value.S)
            .Should()
            .BeEquivalentTo(expected.UniqueIds.Select(value => value.ToString("D")));
        item["contactIdsByRole"]
            .M.ToDictionary(pair => pair.Key, pair => pair.Value.S)
            .Should()
            .Equal(
                expected.ContactIdsByRole.ToDictionary(
                    pair => pair.Key,
                    pair => pair.Value.ToString("D")
                )
            );

        actual.RelatedIds.Should().Equal(expected.RelatedIds);
        actual.LegacyIds.Should().Equal(expected.LegacyIds);
        actual.AlternateIds.Should().Equal(expected.AlternateIds);
        actual.UniqueIds.Should().BeEquivalentTo(expected.UniqueIds);
        actual.ContactIdsByRole.Should().Equal(expected.ContactIdsByRole);
    }

    [Fact]
    public void Canonical_RoundTrip_PreservesFormattedScalarCollections()
    {
        var expected =
            new CanonicalFormattedCollectionModel
            {
                RelatedIds =
                [
                    Guid.Parse("12121212-3434-5656-7878-909090909090"),
                    Guid.Parse("21212121-4343-6565-8787-010101010101"),
                ],
                DurationsByName =
                    new Dictionary<string, TimeSpan>
                    {
                        ["short"] =
                            TimeSpan.Parse("1:02:03.004", CultureInfo.InvariantCulture),
                        ["long"] =
                            TimeSpan.Parse("2:03:04:05.006", CultureInfo.InvariantCulture),
                    },
                Statuses =
                [
                    CanonicalFormattedStatus.Draft, CanonicalFormattedStatus.Published,
                ],
            };

        var item = CanonicalFormattedCollectionModelMapper.ToItem(expected);
        var actual = CanonicalFormattedCollectionModelMapper.FromItem(item);

        item["relatedIds"]
            .L.Select(value => value.S)
            .Should()
            .Equal(expected.RelatedIds.Select(value => value.ToString("N")));
        item["durationsByName"]
            .M.ToDictionary(pair => pair.Key, pair => pair.Value.S)
            .Should()
            .Equal(
                expected.DurationsByName.ToDictionary(
                    pair => pair.Key,
                    pair => pair.Value.ToString("G", CultureInfo.InvariantCulture)
                )
            );
        item["statuses"]
            .L.Select(value => value.S)
            .Should()
            .BeEquivalentTo(
                expected.Statuses.Select(
                    value => ((int)value).ToString(CultureInfo.InvariantCulture)
                )
            );

        actual.RelatedIds.Should().Equal(expected.RelatedIds);
        actual.DurationsByName.Should().Equal(expected.DurationsByName);
        actual.Statuses.Should().BeEquivalentTo(expected.Statuses);
    }

    [Fact]
    public void Canonical_RoundTrip_PreservesBinaryShapes()
    {
        var model = CanonicalModelFactory.CreateModel();

        var item = CanonicalIntegrationModelMapper.ToItem(model);
        var roundTripped = CanonicalIntegrationModelMapper.FromItem(item);

        item["payloadVersions"].L.Should().OnlyContain(value => value.B != null);
        item["payloadByName"].M.Values.Should().OnlyContain(value => value.B != null);
        item["uniquePayloads"].BS.Should().NotBeNull();

        roundTripped.PayloadVersions.Select(Convert.ToBase64String)
            .Should()
            .Equal(model.PayloadVersions.Select(Convert.ToBase64String));
        roundTripped.PayloadByName
            .ToDictionary(pair => pair.Key, pair => Convert.ToBase64String(pair.Value))
            .Should()
            .Equal(
                model.PayloadByName.ToDictionary(
                    pair => pair.Key,
                    pair => Convert.ToBase64String(pair.Value)
                )
            );
        roundTripped.UniquePayloads.Select(Convert.ToBase64String)
            .Should()
            .BeEquivalentTo(model.UniquePayloads.Select(Convert.ToBase64String));

        item["payloadVersions"]
            .L.Select(value => value.B)
            .Should()
            .OnlyContain(stream => stream is MemoryStream);
        item["payloadByName"]
            .M.Values.Select(value => value.B)
            .Should()
            .OnlyContain(stream => stream is MemoryStream);
        item["uniquePayloads"].BS.Should().OnlyContain(stream => stream is MemoryStream);
    }

    [Fact]
    public void Canonical_ToItem_StoresScalarByteArrayAsBinaryAttribute()
    {
        var model = new CanonicalBinaryScalarModel { Payload = new byte[] { 1, 2, 3, 4 } };

        var item = CanonicalBinaryScalarModelMapper.ToItem(model);

        item["payload"].B.Should().NotBeNull();
        item["payload"].B.Should().BeOfType<MemoryStream>();
        item["payload"].B!.ToArray().Should().Equal(model.Payload);
    }

    [Fact]
    public void Canonical_FromItem_ReadsScalarByteArrayFromBinaryAttribute()
    {
        var item =
            new Dictionary<string, AttributeValue>
            {
                ["payload"] = new() { B = new MemoryStream(new byte[] { 5, 6, 7, 8 }) },
            };

        var model = CanonicalBinaryScalarModelMapper.FromItem(item);

        model.Payload.Should().Equal(5, 6, 7, 8);
    }

    [Fact]
    public void Canonical_ToItem_StoresStreamAsBinaryAttribute()
    {
        var model =
            new CanonicalBinaryStreamScalarModel
            {
                Payload = new MemoryStream(new byte[] { 9, 10, 11, 12 }),
            };

        var item = CanonicalBinaryStreamScalarModelMapper.ToItem(model);

        item["payload"].B.Should().NotBeNull();
        item["payload"].B.Should().BeAssignableTo<Stream>();
        item["payload"].B.Should().BeOfType<MemoryStream>();
        item["payload"].B!.ToArray().Should().Equal(9, 10, 11, 12);
    }

    [Fact]
    public void Canonical_FromItem_ReadsStreamFromBinaryAttribute()
    {
        var item =
            new Dictionary<string, AttributeValue>
            {
                ["payload"] = new() { B = new MemoryStream(new byte[] { 13, 14, 15, 16 }) },
            };

        var model = CanonicalBinaryStreamScalarModelMapper.FromItem(item);

        model.Payload.Should().NotBeNull();
        model.Payload.Should().BeAssignableTo<Stream>();
        model.Payload.Should().BeOfType<MemoryStream>();
        ((MemoryStream)model.Payload).ToArray().Should().Equal(13, 14, 15, 16);
    }
}
