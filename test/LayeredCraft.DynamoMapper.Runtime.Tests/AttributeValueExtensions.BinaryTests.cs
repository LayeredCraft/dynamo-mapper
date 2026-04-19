using Amazon.DynamoDBv2.Model;

namespace LayeredCraft.DynamoMapper.Runtime.Tests;

public class AttributeValueExtensionsBinaryTests
{
    [Fact]
    public void GetBinary_ReturnsValue_WhenKeyExists()
    {
        var attributes =
            new Dictionary<string, AttributeValue>
            {
                ["payload"] = new() { B = new MemoryStream(new byte[] { 1, 2, 3 }) },
            };

        var result = attributes.GetBinary("payload");

        Assert.Equal(new byte[] { 1, 2, 3 }, result);
    }

    [Fact]
    public void SetBinary_SetsMemoryStream_WhenNonNullValue()
    {
        var attributes = new Dictionary<string, AttributeValue>();

        attributes.SetBinary("payload", new byte[] { 4, 5, 6 });

        Assert.NotNull(attributes["payload"].B);
        Assert.IsType<MemoryStream>(attributes["payload"].B);
        Assert.Equal(new byte[] { 4, 5, 6 }, attributes["payload"].B!.ToArray());
    }

    [Fact]
    public void SetBinary_OmitsAttribute_WhenEmptyValueAndOmitEmptyStringsTrue()
    {
        var attributes = new Dictionary<string, AttributeValue>();

        attributes.SetBinary("payload", [], true);

        Assert.False(attributes.ContainsKey("payload"));
    }

    [Fact]
    public void GetStream_ReturnsMemoryStream_WhenKeyExists()
    {
        var attributes =
            new Dictionary<string, AttributeValue>
            {
                ["payload"] = new() { B = new MemoryStream(new byte[] { 7, 8, 9 }) },
            };

        var result = attributes.GetStream("payload");

        Assert.IsType<MemoryStream>(result);
        Assert.Equal(new byte[] { 7, 8, 9 }, ((MemoryStream)result).ToArray());
    }

    [Fact]
    public void SetStream_SetsMemoryStream_WhenNonNullValue()
    {
        var attributes = new Dictionary<string, AttributeValue>();

        attributes.SetStream("payload", new MemoryStream(new byte[] { 10, 11, 12 }));

        Assert.NotNull(attributes["payload"].B);
        Assert.IsType<MemoryStream>(attributes["payload"].B);
        Assert.Equal(new byte[] { 10, 11, 12 }, attributes["payload"].B!.ToArray());
    }

    [Fact]
    public void SetStream_PreservesSourcePosition_WhenStreamIsSeekable()
    {
        var attributes = new Dictionary<string, AttributeValue>();
        using var stream = new MemoryStream(new byte[] { 13, 14, 15, 16 });
        stream.Position = 2;

        attributes.SetStream("payload", stream);

        Assert.Equal(2, stream.Position);
        Assert.Equal(new byte[] { 15, 16 }, attributes["payload"].B!.ToArray());
    }
}
