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
