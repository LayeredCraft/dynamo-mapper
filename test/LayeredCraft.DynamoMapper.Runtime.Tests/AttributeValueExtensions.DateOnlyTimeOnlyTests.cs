using System.Globalization;
using Amazon.DynamoDBv2.Model;

namespace LayeredCraft.DynamoMapper.Runtime.Tests;

#if NET6_0_OR_GREATER
public class AttributeValueExtensionsDateOnlyTimeOnlyTests
{
    #region DateOnly Tests

    [Fact]
    public void GetDateOnly_ReturnsValue_WhenKeyExists()
    {
        // Arrange
        var value = new DateOnly(2024, 6, 15);
        var attributes =
            new Dictionary<string, AttributeValue>
            {
                ["startDate"] =
                    new() { S = value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) },
            };

        // Act
        var result = attributes.GetDateOnly("startDate");

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void GetDateOnly_ReturnsMinValue_WhenKeyDoesNotExist()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        var result = attributes.GetDateOnly("startDate", requiredness: Requiredness.Optional);

        // Assert
        Assert.Equal(DateOnly.MinValue, result);
    }

    [Fact]
    public void GetNullableDateOnly_ReturnsValue_WhenKeyExists()
    {
        // Arrange
        var value = new DateOnly(2024, 6, 15);
        var attributes =
            new Dictionary<string, AttributeValue>
            {
                ["startDate"] =
                    new() { S = value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) },
            };

        // Act
        var result = attributes.GetNullableDateOnly("startDate");

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void GetNullableDateOnly_ReturnsNull_WhenKeyDoesNotExist()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        var result = attributes.GetNullableDateOnly("startDate");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void SetDateOnly_ThenGetDateOnly_RoundTrips_WithDefaultFormat()
    {
        // Arrange
        var value = new DateOnly(2024, 6, 15);
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        attributes.SetDateOnly("startDate", value);
        var result = attributes.GetDateOnly("startDate");

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void SetDateOnly_ThenGetDateOnly_RoundTrips_WithCustomFormat()
    {
        // Arrange
        var value = new DateOnly(2024, 6, 15);
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        attributes.SetDateOnly("startDate", value, "yyyyMMdd");
        var result = attributes.GetDateOnly("startDate", "yyyyMMdd");

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void SetDateOnly_NullValue_OmitsKey_WhenOmitNullStringsTrue()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        attributes.SetDateOnly("startDate", null, omitNullStrings: true);

        // Assert
        Assert.False(attributes.ContainsKey("startDate"));
    }

    #endregion

    #region TimeOnly Tests

    [Fact]
    public void GetTimeOnly_ReturnsValue_WhenKeyExists()
    {
        // Arrange
        var value = new TimeOnly(14, 30, 0, 0);
        var attributes =
            new Dictionary<string, AttributeValue>
            {
                ["startTime"] =
                    new()
                    {
                        S =
                            value.ToString(
                                "HH:mm:ss.fffffff",
                                CultureInfo.InvariantCulture
                            ),
                    },
            };

        // Act
        var result = attributes.GetTimeOnly("startTime");

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void GetTimeOnly_ReturnsMinValue_WhenKeyDoesNotExist()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        var result = attributes.GetTimeOnly("startTime", requiredness: Requiredness.Optional);

        // Assert
        Assert.Equal(TimeOnly.MinValue, result);
    }

    [Fact]
    public void GetNullableTimeOnly_ReturnsValue_WhenKeyExists()
    {
        // Arrange
        var value = new TimeOnly(14, 30, 0, 0);
        var attributes =
            new Dictionary<string, AttributeValue>
            {
                ["startTime"] =
                    new()
                    {
                        S =
                            value.ToString(
                                "HH:mm:ss.fffffff",
                                CultureInfo.InvariantCulture
                            ),
                    },
            };

        // Act
        var result = attributes.GetNullableTimeOnly("startTime");

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void GetNullableTimeOnly_ReturnsNull_WhenKeyDoesNotExist()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        var result = attributes.GetNullableTimeOnly("startTime");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void SetTimeOnly_ThenGetTimeOnly_RoundTrips_WithDefaultFormat()
    {
        // Arrange
        var value = new TimeOnly(14, 30, 45, 123, 456);
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        attributes.SetTimeOnly("startTime", value);
        var result = attributes.GetTimeOnly("startTime");

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void SetTimeOnly_ThenGetTimeOnly_RoundTrips_WithCustomFormat()
    {
        // Arrange
        var value = new TimeOnly(14, 30, 0);
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        attributes.SetTimeOnly("startTime", value, "HH:mm");
        var result = attributes.GetTimeOnly("startTime", "HH:mm");

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void SetTimeOnly_NullValue_OmitsKey_WhenOmitNullStringsTrue()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        attributes.SetTimeOnly("startTime", null, omitNullStrings: true);

        // Assert
        Assert.False(attributes.ContainsKey("startTime"));
    }

    #endregion
}
#endif
