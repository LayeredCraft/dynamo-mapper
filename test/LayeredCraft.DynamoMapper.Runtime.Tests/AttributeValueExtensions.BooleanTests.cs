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
            ["isActive"] = new() { BOOL = true },
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
            ["isActive"] = new() { BOOL = false },
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
        var result = attributes.GetBool("isActive", Requiredness.Optional);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetBool_ReturnsFalse_WhenValueIsNull()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["isActive"] = new() { BOOL = null },
        };

        // Act
        var result = attributes.GetBool("isActive", Requiredness.Optional);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetNullableBool_ReturnsTrue_WhenValueIsTrue()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["isActive"] = new() { BOOL = true },
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
            ["isActive"] = new() { BOOL = false },
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
            ["isActive"] = new() { BOOL = null },
        };

        // Act
        var result = attributes.GetNullableBool("isActive");

        // Assert
        Assert.Null(result);
    }

    // SetBool Tests
    [Fact]
    public void SetBool_SetsTrue_WhenValueIsTrue()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        attributes.SetBool("isActive", true);

        // Assert
        Assert.True(attributes.ContainsKey("isActive"));
        Assert.True(attributes["isActive"].BOOL);
    }

    [Fact]
    public void SetBool_SetsFalse_WhenValueIsFalse()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        attributes.SetBool("isActive", false);

        // Assert
        Assert.True(attributes.ContainsKey("isActive"));
        Assert.False(attributes["isActive"].BOOL);
    }

    [Fact]
    public void SetBool_SetsDynamoDBNull_WhenNullValueAndOmitNullStringsFalse()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        attributes.SetBool("isActive", null, omitNullStrings: false);

        // Assert
        Assert.True(attributes.ContainsKey("isActive"));
        Assert.True(attributes["isActive"].NULL);
    }

    [Fact]
    public void SetBool_OmitsAttribute_WhenNullValueAndOmitNullStringsTrue()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        attributes.SetBool("isActive", null, omitNullStrings: true);

        // Assert
        Assert.False(attributes.ContainsKey("isActive"));
    }

    [Fact]
    public void SetBool_ReturnsAttributeDictionary_ForFluentChaining()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        var result = attributes.SetBool("isActive", true);

        // Assert
        Assert.Same(attributes, result);
    }

    [Fact]
    public void SetBool_AllowsFluentChaining_WithMultipleSets()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        attributes
            .SetBool("isActive", true)
            .SetBool("isVerified", false)
            .SetBool("isDeleted", null, omitNullStrings: false);

        // Assert
        Assert.Equal(3, attributes.Count);
        Assert.True(attributes["isActive"].BOOL);
        Assert.False(attributes["isVerified"].BOOL);
        Assert.True(attributes["isDeleted"].NULL);
    }

    // Requiredness Tests
    [Fact]
    public void GetBool_ThrowsException_WhenKeyMissingAndRequired()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            attributes.GetBool("missing", Requiredness.Required)
        );

        Assert.Contains("does not contain an attribute named 'missing'", exception.Message);
    }

    [Fact]
    public void GetNullableBool_ThrowsException_WhenKeyMissingAndRequired()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            attributes.GetNullableBool("missing", Requiredness.Required)
        );

        Assert.Contains("does not contain an attribute named 'missing'", exception.Message);
    }

    // DynamoDB NULL Handling Tests
    [Fact]
    public void GetBool_ReturnsFalse_WhenAttributeHasDynamoDBNull()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["isActive"] = new() { NULL = true },
        };

        // Act
        var result = attributes.GetBool("isActive");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetNullableBool_ReturnsNull_WhenAttributeHasDynamoDBNull()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["isActive"] = new() { NULL = true },
        };

        // Act
        var result = attributes.GetNullableBool("isActive");

        // Assert
        Assert.Null(result);
    }
}
