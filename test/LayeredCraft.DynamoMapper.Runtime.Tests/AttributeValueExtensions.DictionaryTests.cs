using Amazon.DynamoDBv2.Model;

namespace LayeredCraft.DynamoMapper.Runtime.Tests;

public class AttributeValueExtensionsDictionaryTests
{
    [Fact]
    public void SetIfNotNull_SetsAttribute_WhenValueIsNotNull()
    {
        var attributes = new Dictionary<string, AttributeValue>();

        var result =
            attributes.SetIfNotNull("name", "value", value => new AttributeValue { S = value });

        result.Should().BeSameAs(attributes);
        attributes.Should().ContainKey("name");
        attributes["name"].S.Should().Be("value");
    }

    [Fact]
    public void SetIfNotNull_OmitsAttribute_WhenValueIsNull()
    {
        var attributes = new Dictionary<string, AttributeValue>();

        var result =
            attributes.SetIfNotNull<string>(
                "name",
                null,
                value => new AttributeValue { S = value }
            );

        result.Should().BeSameAs(attributes);
        attributes.Should().BeEmpty();
    }
}
