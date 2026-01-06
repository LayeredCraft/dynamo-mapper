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
        var attributes = new Dictionary<string, AttributeValue> { ["name"] = new() { S = value } };

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
        var result = attributes.GetString("name", Requiredness.Optional);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void GetString_ReturnsEmpty_WhenValueIsNull()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["name"] = new() { NULL = true },
        };

        // Act
        var result = attributes.GetString("name", Requiredness.Optional);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Theory]
    [DynamoMapperAutoData]
    public void GetNullableString_ReturnsValue_WhenKeyExists(string value)
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue> { ["name"] = new() { S = value } };

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
        var attributes = new Dictionary<string, AttributeValue> { ["name"] = new() { S = null } };

        // Act
        var result = attributes.GetNullableString("name");

        // Assert
        Assert.Null(result);
    }

    // SetString Tests
    [Theory]
    [DynamoMapperAutoData]
    public void SetString_SetsValue_WhenNonNullValue(string value)
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        attributes.SetString("name", value);

        // Assert
        Assert.True(attributes.ContainsKey("name"));
        Assert.Equal(value, attributes["name"].S);
    }

    [Fact]
    public void SetString_OverwritesExistingValue()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue> { ["name"] = new() { S = "old" } };

        // Act
        attributes.SetString("name", "new");

        // Assert
        Assert.Equal("new", attributes["name"].S);
    }

    [Fact]
    public void SetString_SetsDynamoDBNull_WhenNullValueAndOmitNullStringsFalse()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        attributes.SetString("name", null, omitNullStrings: false);

        // Assert
        Assert.True(attributes.ContainsKey("name"));
        Assert.True(attributes["name"].NULL);
    }

    [Fact]
    public void SetString_OmitsAttribute_WhenNullValueAndOmitNullStringsTrue()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        attributes.SetString("name", null, omitNullStrings: true);

        // Assert
        Assert.False(attributes.ContainsKey("name"));
    }

    [Fact]
    public void SetString_SetsEmptyString_WhenEmptyValueAndOmitEmptyStringsFalse()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        attributes.SetString("name", string.Empty);

        // Assert
        Assert.True(attributes.ContainsKey("name"));
        Assert.Equal(string.Empty, attributes["name"].S);
    }

    [Fact]
    public void SetString_OmitsAttribute_WhenEmptyValueAndOmitEmptyStringsTrue()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        attributes.SetString("name", string.Empty, true);

        // Assert
        Assert.False(attributes.ContainsKey("name"));
    }

    [Fact]
    public void SetString_ReturnsAttributeDictionary_ForFluentChaining()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        var result = attributes.SetString("name", "value");

        // Assert
        Assert.Same(attributes, result);
    }

    [Fact]
    public void SetString_AllowsFluentChaining_WithMultipleSets()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        attributes
            .SetString("first", "value1")
            .SetString("second", "value2")
            .SetString("third", "value3");

        // Assert
        Assert.Equal(3, attributes.Count);
        Assert.Equal("value1", attributes["first"].S);
        Assert.Equal("value2", attributes["second"].S);
        Assert.Equal("value3", attributes["third"].S);
    }

    // Requiredness Tests
    [Fact]
    public void GetString_ThrowsException_WhenKeyMissingAndRequired()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            attributes.GetString("missing", Requiredness.Required)
        );

        Assert.Contains("does not contain an attribute named 'missing'", exception.Message);
    }

    [Fact]
    public void GetString_ReturnsEmpty_WhenKeyMissingAndOptional()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        var result = attributes.GetString("missing", Requiredness.Optional);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void GetNullableString_ThrowsException_WhenKeyMissingAndRequired()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            attributes.GetNullableString("missing", Requiredness.Required)
        );

        Assert.Contains("does not contain an attribute named 'missing'", exception.Message);
    }

    [Fact]
    public void GetNullableString_ReturnsNull_WhenKeyMissingAndOptional()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        var result = attributes.GetNullableString("missing", Requiredness.Optional);

        // Assert
        Assert.Null(result);
    }

    // DynamoDB NULL Handling Tests
    [Fact]
    public void GetString_ReturnsEmpty_WhenAttributeHasDynamoDBNull()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["name"] = new() { NULL = true },
        };

        // Act
        var result = attributes.GetString("name");

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void GetNullableString_ReturnsNull_WhenAttributeHasDynamoDBNull()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["name"] = new() { NULL = true },
        };

        // Act
        var result = attributes.GetNullableString("name");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetString_DistinguishesBetween_MissingKeyAndDynamoDBNull()
    {
        // Arrange - missing key
        var attributesMissingKey = new Dictionary<string, AttributeValue>();

        // Arrange - DynamoDB NULL
        var attributesWithNull = new Dictionary<string, AttributeValue>
        {
            ["name"] = new() { NULL = true },
        };

        // Act
        var resultMissingKey = attributesMissingKey.GetString("name", Requiredness.Optional);
        var resultWithNull = attributesWithNull.GetString("name", Requiredness.Optional);

        // Assert - Both should return empty string, but through different code paths
        Assert.Equal(string.Empty, resultMissingKey);
        Assert.Equal(string.Empty, resultWithNull);
        // The distinction is that one key exists (with NULL=true) and one doesn't
        Assert.False(attributesMissingKey.ContainsKey("name"));
        Assert.True(attributesWithNull.ContainsKey("name"));
    }
}
