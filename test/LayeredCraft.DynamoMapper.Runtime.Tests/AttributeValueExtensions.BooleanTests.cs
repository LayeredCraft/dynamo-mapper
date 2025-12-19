using Amazon.DynamoDBv2.Model;
using DynamoMapper.Runtime;

namespace LayeredCraft.DynamoMapper.Runtime.Tests;

public class AttributeValueExtensionsBooleanTests
{
    [Fact]
    public void GetBool_ReturnsTrue_WhenValueIsTrue()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["isActive"] = new AttributeValue { BOOL = true }
        };

        // Act
        var result = attributes.GetBool("isActive");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GetBool_ReturnsFalse_WhenValueIsFalse()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["isActive"] = new AttributeValue { BOOL = false }
        };

        // Act
        var result = attributes.GetBool("isActive");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetBool_ReturnsFalse_WhenKeyDoesNotExist()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        var result = attributes.GetBool("isActive");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetBool_ReturnsFalse_WhenValueIsNull()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["isActive"] = new AttributeValue { BOOL = null }
        };

        // Act
        var result = attributes.GetBool("isActive");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetNullableBool_ReturnsTrue_WhenValueIsTrue()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["isActive"] = new AttributeValue { BOOL = true }
        };

        // Act
        var result = attributes.GetNullableBool("isActive");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GetNullableBool_ReturnsFalse_WhenValueIsFalse()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["isActive"] = new AttributeValue { BOOL = false }
        };

        // Act
        var result = attributes.GetNullableBool("isActive");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetNullableBool_ReturnsNull_WhenKeyDoesNotExist()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        var result = attributes.GetNullableBool("isActive");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetNullableBool_ReturnsNull_WhenValueIsNull()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["isActive"] = new AttributeValue { BOOL = null }
        };

        // Act
        var result = attributes.GetNullableBool("isActive");

        // Assert
        Assert.Null(result);
    }
}