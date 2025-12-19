using Amazon.DynamoDBv2.Model;
using DynamoMapper.Runtime;
using LayeredCraft.DynamoMapper.TestKit.Attributes;

namespace LayeredCraft.DynamoMapper.Runtime.Tests;

public class AttributeValueExtensionsStringTests
{
    [Theory]
    [DynamoMapperAutoData]
    public void GetString_ReturnsValue_WhenKeyExists(string value)
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["name"] = new AttributeValue { S = value }
        };

        // Act
        var result = attributes.GetString("name");

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void GetString_ReturnsEmpty_WhenKeyDoesNotExist()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        var result = attributes.GetString("name");

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void GetString_ReturnsEmpty_WhenValueIsNull()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["name"] = new AttributeValue { S = null }
        };

        // Act
        var result = attributes.GetString("name");

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Theory]
    [DynamoMapperAutoData]
    public void GetNullableString_ReturnsValue_WhenKeyExists(string value)
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["name"] = new AttributeValue { S = value }
        };

        // Act
        var result = attributes.GetNullableString("name");

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void GetNullableString_ReturnsNull_WhenKeyDoesNotExist()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        var result = attributes.GetNullableString("name");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetNullableString_ReturnsNull_WhenValueIsNull()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["name"] = new AttributeValue { S = null }
        };

        // Act
        var result = attributes.GetNullableString("name");

        // Assert
        Assert.Null(result);
    }
}