using Amazon.DynamoDBv2.Model;
using DynamoMapper.Runtime;
using LayeredCraft.DynamoMapper.TestKit.Attributes;

namespace LayeredCraft.DynamoMapper.Runtime.Tests;

public class AttributeValueExtensionsDateTimeTests
{
    #region DateTime Tests

    [Theory]
    [DynamoMapperAutoData]
    public void GetDateTime_ReturnsValue_WhenKeyExists(DateTime value)
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["createdAt"] = new AttributeValue { S = value.ToString("O") } // ISO-8601 roundtrip format
        };

        // Act
        var result = attributes.GetDateTime("createdAt");

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void GetDateTime_ReturnsMinValue_WhenKeyDoesNotExist()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        var result = attributes.GetDateTime("createdAt");

        // Assert
        Assert.Equal(DateTime.MinValue, result);
    }

    [Fact]
    public void GetDateTime_ReturnsMinValue_WhenValueIsInvalid()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["createdAt"] = new AttributeValue { S = "invalid-date" }
        };

        // Act
        var result = attributes.GetDateTime("createdAt");

        // Assert
        Assert.Equal(DateTime.MinValue, result);
    }

    [Theory]
    [DynamoMapperAutoData]
    public void GetNullableDateTime_ReturnsValue_WhenKeyExists(DateTime value)
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["createdAt"] = new AttributeValue { S = value.ToString("O") }
        };

        // Act
        var result = attributes.GetNullableDateTime("createdAt");

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void GetNullableDateTime_ReturnsNull_WhenKeyDoesNotExist()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        var result = attributes.GetNullableDateTime("createdAt");

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region DateTimeOffset Tests

    [Theory]
    [DynamoMapperAutoData]
    public void GetDateTimeOffset_ReturnsValue_WhenKeyExists(DateTimeOffset value)
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["timestamp"] = new AttributeValue { S = value.ToString("O") }
        };

        // Act
        var result = attributes.GetDateTimeOffset("timestamp");

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void GetDateTimeOffset_ReturnsMinValue_WhenKeyDoesNotExist()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        var result = attributes.GetDateTimeOffset("timestamp");

        // Assert
        Assert.Equal(DateTimeOffset.MinValue, result);
    }

    [Fact]
    public void GetDateTimeOffset_ReturnsMinValue_WhenValueIsInvalid()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["timestamp"] = new AttributeValue { S = "invalid-timestamp" }
        };

        // Act
        var result = attributes.GetDateTimeOffset("timestamp");

        // Assert
        Assert.Equal(DateTimeOffset.MinValue, result);
    }

    [Theory]
    [DynamoMapperAutoData]
    public void GetNullableDateTimeOffset_ReturnsValue_WhenKeyExists(DateTimeOffset value)
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["timestamp"] = new AttributeValue { S = value.ToString("O") }
        };

        // Act
        var result = attributes.GetNullableDateTimeOffset("timestamp");

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void GetNullableDateTimeOffset_ReturnsNull_WhenKeyDoesNotExist()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        var result = attributes.GetNullableDateTimeOffset("timestamp");

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region TimeSpan Tests

    [Fact]
    public void GetTimeSpan_ReturnsValue_WhenKeyExists()
    {
        // Arrange
        var value = TimeSpan.FromHours(2.5);
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["duration"] = new AttributeValue { S = value.ToString() }
        };

        // Act
        var result = attributes.GetTimeSpan("duration");

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void GetTimeSpan_ReturnsZero_WhenKeyDoesNotExist()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        var result = attributes.GetTimeSpan("duration");

        // Assert
        Assert.Equal(TimeSpan.Zero, result);
    }

    [Fact]
    public void GetTimeSpan_ReturnsZero_WhenValueIsInvalid()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["duration"] = new AttributeValue { S = "invalid-timespan" }
        };

        // Act
        var result = attributes.GetTimeSpan("duration");

        // Assert
        Assert.Equal(TimeSpan.Zero, result);
    }

    [Fact]
    public void GetNullableTimeSpan_ReturnsValue_WhenKeyExists()
    {
        // Arrange
        var value = TimeSpan.FromMinutes(30);
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["duration"] = new AttributeValue { S = value.ToString() }
        };

        // Act
        var result = attributes.GetNullableTimeSpan("duration");

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void GetNullableTimeSpan_ReturnsNull_WhenKeyDoesNotExist()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        var result = attributes.GetNullableTimeSpan("duration");

        // Assert
        Assert.Null(result);
    }

    #endregion
}