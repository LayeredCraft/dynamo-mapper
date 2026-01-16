using System.Globalization;
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
            ["createdAt"] = new() { S = value.ToString("O") }, // ISO-8601 roundtrip format
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
        var result = attributes.GetDateTime("createdAt", requiredness: Requiredness.Optional);

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
            ["createdAt"] = new() { S = value.ToString("O") },
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
            ["timestamp"] = new() { S = value.ToString("O") },
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
        var result = attributes.GetDateTimeOffset("timestamp", requiredness: Requiredness.Optional);

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
            ["timestamp"] = new() { S = value.ToString("O") },
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
            ["duration"] = new() { S = value.ToString() },
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
        var result = attributes.GetTimeSpan("duration", requiredness: Requiredness.Optional);

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
            ["duration"] = new() { S = value.ToString("") },
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

    #region DateTime Format Tests

    [Fact]
    public void GetDateTime_WithFormat_YyyyMmDd()
    {
        var dt = new DateTime(2024, 1, 15);
        var attrs = new Dictionary<string, AttributeValue> { ["dt"] = new() { S = "2024-01-15" } };
        Assert.Equal(dt, attrs.GetDateTime("dt", "yyyy-MM-dd"));
    }

    [Fact]
    public void GetDateTime_WithFormat_YyyyMmDdHhMmSs()
    {
        var dt = new DateTime(2024, 1, 15, 10, 30, 45);
        var attrs = new Dictionary<string, AttributeValue>
        {
            ["dt"] = new() { S = "2024-01-15 10:30:45" },
        };
        Assert.Equal(dt, attrs.GetDateTime("dt", "yyyy-MM-dd HH:mm:ss"));
    }

    [Fact]
    public void GetDateTime_WithFormat_YyyyMmDdNoSeparator()
    {
        var dt = new DateTime(2024, 1, 15);
        var attrs = new Dictionary<string, AttributeValue> { ["dt"] = new() { S = "20240115" } };
        Assert.Equal(dt, attrs.GetDateTime("dt", "yyyyMMdd"));
    }

    [Fact]
    public void GetNullableDateTime_WithFormat()
    {
        var dt = new DateTime(2024, 1, 15);
        var attrs = new Dictionary<string, AttributeValue> { ["dt"] = new() { S = "2024-01-15" } };
        Assert.Equal(dt, attrs.GetNullableDateTime("dt", "yyyy-MM-dd"));
    }

    [Fact]
    public void GetNullableDateTime_WithFormat_ReturnsNull_WhenKeyMissing()
    {
        var attrs = new Dictionary<string, AttributeValue>();
        Assert.Null(attrs.GetNullableDateTime("dt", "yyyy-MM-dd"));
    }

    [Fact]
    public void GetNullableDateTime_WithFormat_ReturnsNull_WhenDynamoDBNull()
    {
        var attrs = new Dictionary<string, AttributeValue> { ["dt"] = new() { NULL = true } };
        Assert.Null(attrs.GetNullableDateTime("dt", "yyyy-MM-dd"));
    }

    #endregion

    #region DateTimeOffset Format Tests

    [Fact]
    public void GetDateTimeOffset_WithFormat()
    {
        var dt = new DateTimeOffset(2024, 1, 15, 10, 30, 0, TimeSpan.Zero);
        var attrs = new Dictionary<string, AttributeValue>
        {
            ["ts"] = new() { S = "2024-01-15 10:30:00 +00:00" },
        };
        Assert.Equal(dt, attrs.GetDateTimeOffset("ts", "yyyy-MM-dd HH:mm:ss zzz"));
    }

    [Fact]
    public void GetNullableDateTimeOffset_WithFormat()
    {
        var dt = new DateTimeOffset(2024, 1, 15, 10, 30, 0, TimeSpan.Zero);
        var attrs = new Dictionary<string, AttributeValue>
        {
            ["ts"] = new() { S = "2024-01-15 10:30:00 +00:00" },
        };
        Assert.Equal(dt, attrs.GetNullableDateTimeOffset("ts", "yyyy-MM-dd HH:mm:ss zzz"));
    }

    [Fact]
    public void GetNullableDateTimeOffset_WithFormat_ReturnsNull_WhenMissing()
    {
        var attrs = new Dictionary<string, AttributeValue>();
        Assert.Null(attrs.GetNullableDateTimeOffset("ts", "o"));
    }

    [Fact]
    public void GetNullableDateTimeOffset_WithFormat_ReturnsNull_WhenNull()
    {
        var attrs = new Dictionary<string, AttributeValue> { ["ts"] = new() { NULL = true } };
        Assert.Null(attrs.GetNullableDateTimeOffset("ts", "o"));
    }

    #endregion

    #region TimeSpan Format Tests

    [Fact]
    public void GetTimeSpan_WithFormatC()
    {
        var ts = TimeSpan.FromHours(2.5);
        var attrs = new Dictionary<string, AttributeValue>
        {
            ["dur"] = new() { S = ts.ToString("c") },
        };
        Assert.Equal(ts, attrs.GetTimeSpan("dur", "c"));
    }

    [Fact]
    public void GetTimeSpan_WithFormatG()
    {
        var ts = TimeSpan.FromMinutes(90);
        var attrs = new Dictionary<string, AttributeValue>
        {
            ["dur"] = new() { S = ts.ToString("g") },
        };
        Assert.Equal(ts, attrs.GetTimeSpan("dur", "g"));
    }

    [Fact]
    public void GetNullableTimeSpan_WithFormat()
    {
        var ts = TimeSpan.FromHours(1);
        var attrs = new Dictionary<string, AttributeValue>
        {
            ["dur"] = new() { S = ts.ToString("c") },
        };
        Assert.Equal(ts, attrs.GetNullableTimeSpan("dur", "c"));
    }

    [Fact]
    public void GetNullableTimeSpan_WithFormat_ReturnsNull_WhenMissing()
    {
        var attrs = new Dictionary<string, AttributeValue>();
        Assert.Null(attrs.GetNullableTimeSpan("dur", "c"));
    }

    #endregion

    #region SetDateTime Tests

    [Fact]
    public void SetDateTime_SetsValueWithOFormat()
    {
        var dt = new DateTime(2024, 1, 15, 10, 30, 45, DateTimeKind.Utc);
        var attrs = new Dictionary<string, AttributeValue>();
        attrs.SetDateTime("dt", dt);
        Assert.Equal(dt.ToString("o", CultureInfo.InvariantCulture), attrs["dt"].S);
    }

    [Fact]
    public void SetDateTime_WithCustomFormat()
    {
        var dt = new DateTime(2024, 1, 15);
        var attrs = new Dictionary<string, AttributeValue>();
        attrs.SetDateTime("dt", dt, "yyyy-MM-dd");
        Assert.Equal("2024-01-15", attrs["dt"].S);
    }

    [Fact]
    public void SetDateTime_SetsDynamoDBNull_WhenNullAndOmitFalse()
    {
        var attrs = new Dictionary<string, AttributeValue>();
        attrs.SetDateTime("dt", null, omitNullStrings: false);
        Assert.True(attrs["dt"].NULL);
    }

    [Fact]
    public void SetDateTime_OmitsAttribute_WhenNullAndOmitTrue()
    {
        var attrs = new Dictionary<string, AttributeValue>();
        attrs.SetDateTime("dt", null, omitNullStrings: true);
        Assert.False(attrs.ContainsKey("dt"));
    }

    [Fact]
    public void SetDateTime_ReturnsDict_ForFluentChaining()
    {
        var dt = DateTime.Now;
        var attrs = new Dictionary<string, AttributeValue>();
        var result = attrs.SetDateTime("dt", dt);
        Assert.Same(attrs, result);
    }

    #endregion

    #region SetDateTimeOffset Tests

    [Fact]
    public void SetDateTimeOffset_SetsValue()
    {
        var dt = DateTimeOffset.Now;
        var attrs = new Dictionary<string, AttributeValue>();
        attrs.SetDateTimeOffset("ts", dt);
        Assert.Equal(dt.ToString("o", CultureInfo.InvariantCulture), attrs["ts"].S);
    }

    [Fact]
    public void SetDateTimeOffset_WithCustomFormat()
    {
        var dt = new DateTimeOffset(2024, 1, 15, 10, 30, 0, TimeSpan.Zero);
        var attrs = new Dictionary<string, AttributeValue>();
        attrs.SetDateTimeOffset("ts", dt, "yyyy-MM-dd HH:mm:ss zzz");
        Assert.Contains("2024-01-15", attrs["ts"].S);
    }

    [Fact]
    public void SetDateTimeOffset_SetsDynamoDBNull_WhenNull()
    {
        var attrs = new Dictionary<string, AttributeValue>();
        attrs.SetDateTimeOffset("ts", null, omitNullStrings: false);
        Assert.True(attrs["ts"].NULL);
    }

    [Fact]
    public void SetDateTimeOffset_OmitsAttribute_WhenNull()
    {
        var attrs = new Dictionary<string, AttributeValue>();
        attrs.SetDateTimeOffset("ts", null, omitNullStrings: true);
        Assert.False(attrs.ContainsKey("ts"));
    }

    #endregion

    #region SetTimeSpan Tests

    [Fact]
    public void SetTimeSpan_SetsValue()
    {
        var ts = TimeSpan.FromHours(2);
        var attrs = new Dictionary<string, AttributeValue>();
        attrs.SetTimeSpan("dur", ts);
        Assert.Equal(ts.ToString("c", CultureInfo.InvariantCulture), attrs["dur"].S);
    }

    [Fact]
    public void SetTimeSpan_WithCustomFormat()
    {
        var ts = TimeSpan.FromMinutes(90);
        var attrs = new Dictionary<string, AttributeValue>();
        attrs.SetTimeSpan("dur", ts, "g");
        Assert.Equal(ts.ToString("g", CultureInfo.InvariantCulture), attrs["dur"].S);
    }

    [Fact]
    public void SetTimeSpan_SetsDynamoDBNull_WhenNull()
    {
        var attrs = new Dictionary<string, AttributeValue>();
        attrs.SetTimeSpan("dur", null, omitNullStrings: false);
        Assert.True(attrs["dur"].NULL);
    }

    [Fact]
    public void SetTimeSpan_OmitsAttribute_WhenNull()
    {
        var attrs = new Dictionary<string, AttributeValue>();
        attrs.SetTimeSpan("dur", null, omitNullStrings: true);
        Assert.False(attrs.ContainsKey("dur"));
    }

    #endregion

    #region DynamoDB NULL and Requiredness

    [Fact]
    public void GetDateTime_ReturnsMinValue_WhenAttributeHasDynamoDBNull()
    {
        var attrs = new Dictionary<string, AttributeValue> { ["dt"] = new() { NULL = true } };
        Assert.Equal(
            DateTime.MinValue,
            attrs.GetDateTime("dt", requiredness: Requiredness.Optional)
        );
    }

    [Fact]
    public void GetNullableDateTime_ReturnsNull_WhenAttributeHasDynamoDBNull()
    {
        var attrs = new Dictionary<string, AttributeValue> { ["dt"] = new() { NULL = true } };
        Assert.Null(attrs.GetNullableDateTime("dt"));
    }

    [Fact]
    public void GetDateTime_ThrowsException_WhenKeyMissingAndRequired()
    {
        var attrs = new Dictionary<string, AttributeValue>();
        var ex = Assert.Throws<InvalidOperationException>(() =>
            attrs.GetDateTime("missing", requiredness: Requiredness.Required)
        );
        Assert.Contains("does not contain an attribute named 'missing'", ex.Message);
    }

    #endregion
}
