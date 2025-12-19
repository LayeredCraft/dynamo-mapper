using Amazon.DynamoDBv2.Model;
using DynamoMapper.Runtime;
using LayeredCraft.DynamoMapper.TestKit.Attributes;

namespace LayeredCraft.DynamoMapper.Runtime.Tests;

public class AttributeValueExtensionsGuidTests
{
    [Theory]
    [DynamoMapperAutoData]
    public void GetGuid_ReturnsValue_WhenKeyExists(Guid value)
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["id"] = new AttributeValue { S = value.ToString() }
        };

        // Act
        var result = attributes.GetGuid("id");

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void GetGuid_ReturnsEmpty_WhenKeyDoesNotExist()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        var result = attributes.GetGuid("id");

        // Assert
        Assert.Equal(Guid.Empty, result);
    }

    [Fact]
    public void GetGuid_ReturnsEmpty_WhenValueIsInvalid()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["id"] = new AttributeValue { S = "invalid-guid" }
        };

        // Act
        var result = attributes.GetGuid("id");

        // Assert
        Assert.Equal(Guid.Empty, result);
    }

    [Fact]
    public void GetGuid_ReturnsEmpty_WhenValueIsNull()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["id"] = new AttributeValue { S = null }
        };

        // Act
        var result = attributes.GetGuid("id");

        // Assert
        Assert.Equal(Guid.Empty, result);
    }

    [Theory]
    [DynamoMapperAutoData]
    public void GetNullableGuid_ReturnsValue_WhenKeyExists(Guid value)
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["id"] = new AttributeValue { S = value.ToString() }
        };

        // Act
        var result = attributes.GetNullableGuid("id");

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void GetNullableGuid_ReturnsNull_WhenKeyDoesNotExist()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        var result = attributes.GetNullableGuid("id");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetNullableGuid_ReturnsNull_WhenValueIsInvalid()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["id"] = new AttributeValue { S = "invalid-guid" }
        };

        // Act
        var result = attributes.GetNullableGuid("id");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetNullableGuid_ReturnsNull_WhenValueIsNull()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["id"] = new AttributeValue { S = null }
        };

        // Act
        var result = attributes.GetNullableGuid("id");

        // Assert
        Assert.Null(result);
    }
}