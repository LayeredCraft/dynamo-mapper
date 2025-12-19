using Amazon.DynamoDBv2.Model;
using DynamoMapper.Runtime;

namespace LayeredCraft.DynamoMapper.Runtime.Tests;

public class AttributeValueExtensionsEnumTests
{
    public enum TestStatus
    {
        Pending,
        Active,
        Completed,
        Cancelled
    }

    [Fact]
    public void GetEnum_ReturnsValue_WhenKeyExists()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["status"] = new AttributeValue { S = "Active" }
        };

        // Act
        var result = attributes.GetEnum("status", TestStatus.Pending);

        // Assert
        Assert.Equal(TestStatus.Active, result);
    }

    [Fact]
    public void GetEnum_ReturnsDefault_WhenKeyDoesNotExist()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        var result = attributes.GetEnum("status", TestStatus.Completed);

        // Assert
        Assert.Equal(TestStatus.Completed, result);
    }

    [Fact]
    public void GetEnum_ReturnsDefault_WhenValueIsInvalid()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["status"] = new AttributeValue { S = "InvalidStatus" }
        };

        // Act
        var result = attributes.GetEnum("status", TestStatus.Cancelled);

        // Assert
        Assert.Equal(TestStatus.Cancelled, result);
    }

    [Fact]
    public void GetEnum_ReturnsDefault_WhenValueIsNull()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["status"] = new AttributeValue { S = null }
        };

        // Act
        var result = attributes.GetEnum("status", TestStatus.Pending);

        // Assert
        Assert.Equal(TestStatus.Pending, result);
    }

    [Fact]
    public void GetNullableEnum_ReturnsValue_WhenKeyExists()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["status"] = new AttributeValue { S = "Completed" }
        };

        // Act
        var result = attributes.GetNullableEnum<TestStatus>("status");

        // Assert
        Assert.Equal(TestStatus.Completed, result);
    }

    [Fact]
    public void GetNullableEnum_ReturnsNull_WhenKeyDoesNotExist()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        var result = attributes.GetNullableEnum<TestStatus>("status");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetNullableEnum_ReturnsNull_WhenValueIsInvalid()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["status"] = new AttributeValue { S = "InvalidStatus" }
        };

        // Act
        var result = attributes.GetNullableEnum<TestStatus>("status");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetNullableEnum_ReturnsNull_WhenValueIsNull()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["status"] = new AttributeValue { S = null }
        };

        // Act
        var result = attributes.GetNullableEnum<TestStatus>("status");

        // Assert
        Assert.Null(result);
    }
}