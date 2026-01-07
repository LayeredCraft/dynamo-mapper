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
        Cancelled,
    }

    [Fact]
    public void GetEnum_ReturnsValue_WhenKeyExists()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["status"] = new() { S = "Active" },
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
        var result = attributes.GetEnum("status", TestStatus.Completed, Requiredness.Optional);

        // Assert
        Assert.Equal(TestStatus.Completed, result);
    }

    [Fact]
    public void GetEnum_ReturnsDefault_WhenValueIsNull()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["status"] = new() { NULL = true },
        };

        // Act
        var result = attributes.GetEnum("status", TestStatus.Pending, Requiredness.Optional);

        // Assert
        Assert.Equal(TestStatus.Pending, result);
    }

    [Fact]
    public void GetNullableEnum_ReturnsValue_WhenKeyExists()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["status"] = new() { S = "Completed" },
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
    public void GetNullableEnum_ReturnsNull_WhenValueIsNull()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["status"] = new() { NULL = true },
        };

        // Act
        var result = attributes.GetNullableEnum<TestStatus>("status");

        // Assert
        Assert.Null(result);
    }

    // GetEnum Format Overload Tests
    [Fact]
    public void GetEnum_WithFormatG_ParsesName()
    {
        var attrs = new Dictionary<string, AttributeValue> { ["status"] = new() { S = "Active" } };
        Assert.Equal(TestStatus.Active, attrs.GetEnum("status", TestStatus.Pending, "G"));
    }

    [Fact]
    public void GetEnum_WithFormatD_ParsesDecimal()
    {
        var attrs = new Dictionary<string, AttributeValue> { ["status"] = new() { S = "1" } };
        Assert.Equal(TestStatus.Active, attrs.GetEnum("status", TestStatus.Pending, "D"));
    }

    [Fact]
    public void GetEnum_WithFormatX_ParsesHex()
    {
        var attrs = new Dictionary<string, AttributeValue>
        {
            ["status"] = new() { S = "00000001" },
        };
        Assert.Equal(TestStatus.Active, attrs.GetEnum("status", TestStatus.Pending, "X"));
    }

    [Fact]
    public void GetNullableEnum_WithFormat_ParsesCorrectly()
    {
        var attrs = new Dictionary<string, AttributeValue> { ["status"] = new() { S = "Active" } };
        Assert.Equal(TestStatus.Active, attrs.GetNullableEnum<TestStatus>("status", "G"));
    }

    [Fact]
    public void GetNullableEnum_WithFormat_ReturnsNull_WhenKeyMissing()
    {
        var attrs = new Dictionary<string, AttributeValue>();
        Assert.Null(attrs.GetNullableEnum<TestStatus>("status", "G"));
    }

    // SetEnum Tests
    [Fact]
    public void SetEnum_SetsValue()
    {
        var attrs = new Dictionary<string, AttributeValue>();
        attrs.SetEnum<TestStatus>("status", TestStatus.Active);
        Assert.Equal("Active", attrs["status"].S);
    }

    [Fact]
    public void SetEnum_WithFormatG_FormatsAsName()
    {
        var attrs = new Dictionary<string, AttributeValue>();
        attrs.SetEnum<TestStatus>("status", TestStatus.Active, "G");
        Assert.Equal("Active", attrs["status"].S);
    }

    [Fact]
    public void SetEnum_WithFormatD_FormatsAsDecimal()
    {
        var attrs = new Dictionary<string, AttributeValue>();
        attrs.SetEnum<TestStatus>("status", TestStatus.Active, "D");
        Assert.Equal("1", attrs["status"].S);
    }

    [Fact]
    public void SetEnum_WithFormatX_FormatsAsHex()
    {
        var attrs = new Dictionary<string, AttributeValue>();
        attrs.SetEnum<TestStatus>("status", TestStatus.Active, "X");
        Assert.Equal("00000001", attrs["status"].S);
    }

    [Fact]
    public void SetEnum_SetsDynamoDBNull_WhenNullAndOmitFalse()
    {
        var attrs = new Dictionary<string, AttributeValue>();
        attrs.SetEnum<TestStatus>("status", null, omitNullStrings: false);
        Assert.True(attrs["status"].NULL);
    }

    [Fact]
    public void SetEnum_OmitsAttribute_WhenNullAndOmitTrue()
    {
        var attrs = new Dictionary<string, AttributeValue>();
        attrs.SetEnum<TestStatus>("status", null, omitNullStrings: true);
        Assert.False(attrs.ContainsKey("status"));
    }

    [Fact]
    public void SetEnum_ReturnsDict_ForFluentChaining()
    {
        var attrs = new Dictionary<string, AttributeValue>();
        var result = attrs.SetEnum<TestStatus>("status", TestStatus.Active);
        Assert.Same(attrs, result);
    }

    [Fact]
    public void SetEnum_AllowsMultipleEnums()
    {
        var attrs = new Dictionary<string, AttributeValue>();
        attrs
            .SetEnum<TestStatus>("status", TestStatus.Active)
            .SetEnum<TestStatus>("prev", TestStatus.Pending);
        Assert.Equal(2, attrs.Count);
    }

    // Requiredness Tests
    [Fact]
    public void GetEnum_ThrowsException_WhenKeyMissingAndRequired()
    {
        var attrs = new Dictionary<string, AttributeValue>();
        var ex = Assert.Throws<InvalidOperationException>(() =>
            attrs.GetEnum("missing", TestStatus.Pending, Requiredness.Required)
        );
        Assert.Contains("does not contain an attribute named 'missing'", ex.Message);
    }

    [Fact]
    public void GetNullableEnum_ThrowsException_WhenKeyMissingAndRequired()
    {
        var attrs = new Dictionary<string, AttributeValue>();
        var ex = Assert.Throws<InvalidOperationException>(() =>
            attrs.GetNullableEnum<TestStatus>("missing", Requiredness.Required)
        );
        Assert.Contains("does not contain an attribute named 'missing'", ex.Message);
    }
}
