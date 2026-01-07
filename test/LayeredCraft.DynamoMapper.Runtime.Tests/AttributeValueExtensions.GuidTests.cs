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
            ["id"] = new() { S = value.ToString() },
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
        var result = attributes.GetGuid("id", Requiredness.Optional);

        // Assert
        Assert.Equal(Guid.Empty, result);
    }

    [Fact]
    public void GetGuid_ReturnsEmpty_WhenValueIsNull()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue> { ["id"] = new() { NULL = true } };

        // Act
        var result = attributes.GetGuid("id", Requiredness.Optional);

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
            ["id"] = new() { S = value.ToString() },
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
    public void GetNullableGuid_ReturnsNull_WhenValueIsNull()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue> { ["id"] = new() { NULL = true } };

        // Act
        var result = attributes.GetNullableGuid("id");

        // Assert
        Assert.Null(result);
    }

    // GetGuid Format Overload Tests
    [Fact]
    public void GetGuid_WithFormatN_ParsesCorrectly()
    {
        var guid = Guid.NewGuid();
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["id"] = new() { S = guid.ToString("N") },
        };
        Assert.Equal(guid, attributes.GetGuid("id", "N"));
    }

    [Fact]
    public void GetGuid_WithFormatD_ParsesCorrectly()
    {
        var guid = Guid.NewGuid();
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["id"] = new() { S = guid.ToString("D") },
        };
        Assert.Equal(guid, attributes.GetGuid("id", "D"));
    }

    [Fact]
    public void GetGuid_WithFormatB_ParsesCorrectly()
    {
        var guid = Guid.NewGuid();
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["id"] = new() { S = guid.ToString("B") },
        };
        Assert.Equal(guid, attributes.GetGuid("id", "B"));
    }

    [Fact]
    public void GetGuid_WithFormatP_ParsesCorrectly()
    {
        var guid = Guid.NewGuid();
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["id"] = new() { S = guid.ToString("P") },
        };
        Assert.Equal(guid, attributes.GetGuid("id", "P"));
    }

    [Fact]
    public void GetGuid_WithFormatX_ParsesCorrectly()
    {
        var guid = Guid.NewGuid();
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["id"] = new() { S = guid.ToString("X") },
        };
        Assert.Equal(guid, attributes.GetGuid("id", "X"));
    }

    // GetNullableGuid Format Overload Tests
    [Fact]
    public void GetNullableGuid_WithFormatN_ParsesCorrectly()
    {
        var guid = Guid.NewGuid();
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["id"] = new() { S = guid.ToString("N") },
        };
        Assert.Equal(guid, attributes.GetNullableGuid("id", "N"));
    }

    [Fact]
    public void GetNullableGuid_WithFormatD_ParsesCorrectly()
    {
        var guid = Guid.NewGuid();
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["id"] = new() { S = guid.ToString("D") },
        };
        Assert.Equal(guid, attributes.GetNullableGuid("id", "D"));
    }

    [Fact]
    public void GetNullableGuid_WithFormatB_ParsesCorrectly()
    {
        var guid = Guid.NewGuid();
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["id"] = new() { S = guid.ToString("B") },
        };
        Assert.Equal(guid, attributes.GetNullableGuid("id", "B"));
    }

    [Fact]
    public void GetNullableGuid_WithFormat_ReturnsNull_WhenKeyMissing()
    {
        var attributes = new Dictionary<string, AttributeValue>();
        Assert.Null(attributes.GetNullableGuid("id", "D"));
    }

    [Fact]
    public void GetNullableGuid_WithFormat_ReturnsNull_WhenDynamoDBNull()
    {
        var attributes = new Dictionary<string, AttributeValue> { ["id"] = new() { NULL = true } };
        Assert.Null(attributes.GetNullableGuid("id", "D"));
    }

    // SetGuid Tests
    [Fact]
    public void SetGuid_SetsValue_WhenNonNullValue()
    {
        var guid = Guid.NewGuid();
        var attributes = new Dictionary<string, AttributeValue>();
        attributes.SetGuid("id", guid);
        Assert.True(attributes.ContainsKey("id"));
        Assert.Equal(guid.ToString("D"), attributes["id"].S);
    }

    [Fact]
    public void SetGuid_WithFormatN_FormatsCorrectly()
    {
        var guid = Guid.NewGuid();
        var attributes = new Dictionary<string, AttributeValue>();
        attributes.SetGuid("id", guid, "N");
        Assert.Equal(guid.ToString("N"), attributes["id"].S);
    }

    [Fact]
    public void SetGuid_WithFormatD_FormatsCorrectly()
    {
        var guid = Guid.NewGuid();
        var attributes = new Dictionary<string, AttributeValue>();
        attributes.SetGuid("id", guid, "D");
        Assert.Equal(guid.ToString("D"), attributes["id"].S);
    }

    [Fact]
    public void SetGuid_WithFormatB_FormatsCorrectly()
    {
        var guid = Guid.NewGuid();
        var attributes = new Dictionary<string, AttributeValue>();
        attributes.SetGuid("id", guid, "B");
        Assert.Equal(guid.ToString("B"), attributes["id"].S);
    }

    [Fact]
    public void SetGuid_WithFormatP_FormatsCorrectly()
    {
        var guid = Guid.NewGuid();
        var attributes = new Dictionary<string, AttributeValue>();
        attributes.SetGuid("id", guid, "P");
        Assert.Equal(guid.ToString("P"), attributes["id"].S);
    }

    [Fact]
    public void SetGuid_SetsDynamoDBNull_WhenNullValueAndOmitNullStringsFalse()
    {
        var attributes = new Dictionary<string, AttributeValue>();
        attributes.SetGuid("id", null, omitNullStrings: false);
        Assert.True(attributes["id"].NULL);
    }

    [Fact]
    public void SetGuid_OmitsAttribute_WhenNullValueAndOmitNullStringsTrue()
    {
        var attributes = new Dictionary<string, AttributeValue>();
        attributes.SetGuid("id", null, omitNullStrings: true);
        Assert.False(attributes.ContainsKey("id"));
    }

    [Fact]
    public void SetGuid_ReturnsAttributeDictionary_ForFluentChaining()
    {
        var guid = Guid.NewGuid();
        var attributes = new Dictionary<string, AttributeValue>();
        var result = attributes.SetGuid("id", guid);
        Assert.Same(attributes, result);
    }

    // Requiredness Tests
    [Fact]
    public void GetGuid_WithFormat_ThrowsException_WhenKeyMissingAndRequired()
    {
        var attributes = new Dictionary<string, AttributeValue>();
        var ex = Assert.Throws<InvalidOperationException>(() =>
            attributes.GetGuid("missing", "D", Requiredness.Required)
        );
        Assert.Contains("does not contain an attribute named 'missing'", ex.Message);
    }

    [Fact]
    public void GetNullableGuid_WithFormat_ThrowsException_WhenKeyMissingAndRequired()
    {
        var attributes = new Dictionary<string, AttributeValue>();
        var ex = Assert.Throws<InvalidOperationException>(() =>
            attributes.GetNullableGuid("missing", "D", Requiredness.Required)
        );
        Assert.Contains("does not contain an attribute named 'missing'", ex.Message);
    }
}
