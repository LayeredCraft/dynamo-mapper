using System.Globalization;
using Amazon.DynamoDBv2.Model;
using DynamoMapper.Runtime;
using LayeredCraft.DynamoMapper.TestKit.Attributes;

namespace LayeredCraft.DynamoMapper.Runtime.Tests;

public class AttributeValueExtensionsNumericTests
{
    #region Int Tests

    [Theory]
    [InlineDynamoMapperAutoData(42)]
    [InlineDynamoMapperAutoData(0)]
    [InlineDynamoMapperAutoData(-100)]
    public void GetInt_ReturnsValue_WhenKeyExists(int value)
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["count"] = new() { N = value.ToString() },
        };

        // Act
        var result = attributes.GetInt("count");

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void GetInt_ReturnsZero_WhenKeyDoesNotExist()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        var result = attributes.GetInt("count", Requiredness.Optional);

        // Assert
        Assert.Equal(0, result);
    }

    [Theory]
    [InlineDynamoMapperAutoData(42)]
    [InlineDynamoMapperAutoData(-100)]
    public void GetNullableInt_ReturnsValue_WhenKeyExists(int value)
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["count"] = new() { N = value.ToString() },
        };

        // Act
        var result = attributes.GetNullableInt("count");

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void GetNullableInt_ReturnsNull_WhenKeyDoesNotExist()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        var result = attributes.GetNullableInt("count");

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region Long Tests

    [Theory]
    [InlineDynamoMapperAutoData(123456789L)]
    [InlineDynamoMapperAutoData(0L)]
    [InlineDynamoMapperAutoData(-987654321L)]
    public void GetLong_ReturnsValue_WhenKeyExists(long value)
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["bigNumber"] = new() { N = value.ToString() },
        };

        // Act
        var result = attributes.GetLong("bigNumber");

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void GetLong_ReturnsZero_WhenKeyDoesNotExist()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        var result = attributes.GetLong("bigNumber", Requiredness.Optional);

        // Assert
        Assert.Equal(0L, result);
    }

    [Theory]
    [InlineDynamoMapperAutoData(123456789L)]
    public void GetNullableLong_ReturnsValue_WhenKeyExists(long value)
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["bigNumber"] = new() { N = value.ToString() },
        };

        // Act
        var result = attributes.GetNullableLong("bigNumber");

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void GetNullableLong_ReturnsNull_WhenKeyDoesNotExist()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        var result = attributes.GetNullableLong("bigNumber");

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region Float Tests

    [Theory]
    [InlineDynamoMapperAutoData(3.14f)]
    [InlineDynamoMapperAutoData(0f)]
    [InlineDynamoMapperAutoData(-2.5f)]
    public void GetFloat_ReturnsValue_WhenKeyExists(float value)
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["ratio"] = new() { N = value.ToString(CultureInfo.InvariantCulture) },
        };

        // Act
        var result = attributes.GetFloat("ratio");

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void GetFloat_ReturnsZero_WhenKeyDoesNotExist()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        var result = attributes.GetFloat("ratio", Requiredness.Optional);

        // Assert
        Assert.Equal(0f, result);
    }

    [Theory]
    [InlineDynamoMapperAutoData(3.14f)]
    public void GetNullableFloat_ReturnsValue_WhenKeyExists(float value)
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["ratio"] = new() { N = value.ToString(CultureInfo.InvariantCulture) },
        };

        // Act
        var result = attributes.GetNullableFloat("ratio");

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void GetNullableFloat_ReturnsNull_WhenKeyDoesNotExist()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        var result = attributes.GetNullableFloat("ratio");

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region Double Tests

    [Theory]
    [InlineDynamoMapperAutoData(3.14159)]
    [InlineDynamoMapperAutoData(0.0)]
    [InlineDynamoMapperAutoData(-2.71828)]
    public void GetDouble_ReturnsValue_WhenKeyExists(double value)
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["measurement"] = new() { N = value.ToString(CultureInfo.InvariantCulture) },
        };

        // Act
        var result = attributes.GetDouble("measurement");

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void GetDouble_ReturnsZero_WhenKeyDoesNotExist()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        var result = attributes.GetDouble("measurement", Requiredness.Optional);

        // Assert
        Assert.Equal(0.0, result);
    }

    [Theory]
    [InlineDynamoMapperAutoData(3.14159)]
    public void GetNullableDouble_ReturnsValue_WhenKeyExists(double value)
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["measurement"] = new() { N = value.ToString(CultureInfo.InvariantCulture) },
        };

        // Act
        var result = attributes.GetNullableDouble("measurement");

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void GetNullableDouble_ReturnsNull_WhenKeyDoesNotExist()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        var result = attributes.GetNullableDouble("measurement");

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region Decimal Tests

    [Fact]
    public void GetDecimal_ReturnsValue_WhenKeyExists()
    {
        // Arrange
        var value = 99.99m;
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["price"] = new() { N = value.ToString(CultureInfo.InvariantCulture) },
        };

        // Act
        var result = attributes.GetDecimal("price");

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void GetDecimal_ReturnsZero_WhenKeyDoesNotExist()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        var result = attributes.GetDecimal("price", Requiredness.Optional);

        // Assert
        Assert.Equal(0m, result);
    }

    [Fact]
    public void GetNullableDecimal_ReturnsValue_WhenKeyExists()
    {
        // Arrange
        var value = 99.99m;
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["price"] = new() { N = value.ToString(CultureInfo.InvariantCulture) },
        };

        // Act
        var result = attributes.GetNullableDecimal("price");

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void GetNullableDecimal_ReturnsNull_WhenKeyDoesNotExist()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        var result = attributes.GetNullableDecimal("price");

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region SetInt Tests

    [Fact]
    public void SetInt_SetsValue_WhenNonNullValue()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        attributes.SetInt("count", 42);

        // Assert
        Assert.True(attributes.ContainsKey("count"));
        Assert.Equal("42", attributes["count"].N);
    }

    [Fact]
    public void SetInt_SetsZero()
    {
        var attributes = new Dictionary<string, AttributeValue>();
        attributes.SetInt("count", 0);
        Assert.Equal("0", attributes["count"].N);
    }

    [Fact]
    public void SetInt_SetsNegative()
    {
        var attributes = new Dictionary<string, AttributeValue>();
        attributes.SetInt("temp", -100);
        Assert.Equal("-100", attributes["temp"].N);
    }

    [Fact]
    public void SetInt_SetsDynamoDBNull_WhenNullAndOmitFalse()
    {
        var attributes = new Dictionary<string, AttributeValue>();
        attributes.SetInt("count", null, omitNullStrings: false);
        Assert.True(attributes["count"].NULL);
    }

    [Fact]
    public void SetInt_OmitsAttribute_WhenNullAndOmitTrue()
    {
        var attributes = new Dictionary<string, AttributeValue>();
        attributes.SetInt("count", null, omitNullStrings: true);
        Assert.False(attributes.ContainsKey("count"));
    }

    [Fact]
    public void SetInt_ReturnsDict_ForFluentChaining()
    {
        var attributes = new Dictionary<string, AttributeValue>();
        var result = attributes.SetInt("count", 42);
        Assert.Same(attributes, result);
    }

    #endregion

    #region SetLong Tests

    [Fact]
    public void SetLong_SetsValue()
    {
        var attributes = new Dictionary<string, AttributeValue>();
        attributes.SetLong("id", 123456789L);
        Assert.Equal("123456789", attributes["id"].N);
    }

    [Fact]
    public void SetLong_SetsZero()
    {
        var attributes = new Dictionary<string, AttributeValue>();
        attributes.SetLong("id", 0L);
        Assert.Equal("0", attributes["id"].N);
    }

    [Fact]
    public void SetLong_SetsNegative()
    {
        var attributes = new Dictionary<string, AttributeValue>();
        attributes.SetLong("offset", -987654321L);
        Assert.Equal("-987654321", attributes["offset"].N);
    }

    [Fact]
    public void SetLong_SetsDynamoDBNull_WhenNullAndOmitFalse()
    {
        var attributes = new Dictionary<string, AttributeValue>();
        attributes.SetLong("id", null, omitNullStrings: false);
        Assert.True(attributes["id"].NULL);
    }

    [Fact]
    public void SetLong_OmitsAttribute_WhenNullAndOmitTrue()
    {
        var attributes = new Dictionary<string, AttributeValue>();
        attributes.SetLong("id", null, omitNullStrings: true);
        Assert.False(attributes.ContainsKey("id"));
    }

    #endregion

    #region SetFloat Tests

    [Fact]
    public void SetFloat_SetsValue()
    {
        var attributes = new Dictionary<string, AttributeValue>();
        attributes.SetFloat("ratio", 3.14f);
        Assert.Equal("3.14", attributes["ratio"].N);
    }

    [Fact]
    public void SetFloat_UsesInvariantCulture()
    {
        var attributes = new Dictionary<string, AttributeValue>();
        attributes.SetFloat("value", 1.5f);
        Assert.Equal("1.5", attributes["value"].N);
        Assert.Contains(".", attributes["value"].N);
    }

    [Fact]
    public void SetFloat_SetsZero()
    {
        var attributes = new Dictionary<string, AttributeValue>();
        attributes.SetFloat("ratio", 0f);
        Assert.Equal("0", attributes["ratio"].N);
    }

    [Fact]
    public void SetFloat_SetsNegative()
    {
        var attributes = new Dictionary<string, AttributeValue>();
        attributes.SetFloat("adj", -2.5f);
        Assert.Equal("-2.5", attributes["adj"].N);
    }

    [Fact]
    public void SetFloat_SetsDynamoDBNull_WhenNullAndOmitFalse()
    {
        var attributes = new Dictionary<string, AttributeValue>();
        attributes.SetFloat("ratio", null, omitNullStrings: false);
        Assert.True(attributes["ratio"].NULL);
    }

    [Fact]
    public void SetFloat_OmitsAttribute_WhenNullAndOmitTrue()
    {
        var attributes = new Dictionary<string, AttributeValue>();
        attributes.SetFloat("ratio", null, omitNullStrings: true);
        Assert.False(attributes.ContainsKey("ratio"));
    }

    #endregion

    #region SetDouble Tests

    [Fact]
    public void SetDouble_SetsValue()
    {
        var attributes = new Dictionary<string, AttributeValue>();
        attributes.SetDouble("pi", 3.14159);
        Assert.Equal("3.14159", attributes["pi"].N);
    }

    [Fact]
    public void SetDouble_UsesInvariantCulture()
    {
        var attributes = new Dictionary<string, AttributeValue>();
        attributes.SetDouble("value", 1.5);
        Assert.Equal("1.5", attributes["value"].N);
        Assert.Contains(".", attributes["value"].N);
    }

    [Fact]
    public void SetDouble_SetsZero()
    {
        var attributes = new Dictionary<string, AttributeValue>();
        attributes.SetDouble("value", 0.0);
        Assert.Equal("0", attributes["value"].N);
    }

    [Fact]
    public void SetDouble_SetsNegative()
    {
        var attributes = new Dictionary<string, AttributeValue>();
        attributes.SetDouble("e", -2.71828);
        Assert.Equal("-2.71828", attributes["e"].N);
    }

    [Fact]
    public void SetDouble_SetsDynamoDBNull_WhenNullAndOmitFalse()
    {
        var attributes = new Dictionary<string, AttributeValue>();
        attributes.SetDouble("value", null, omitNullStrings: false);
        Assert.True(attributes["value"].NULL);
    }

    [Fact]
    public void SetDouble_OmitsAttribute_WhenNullAndOmitTrue()
    {
        var attributes = new Dictionary<string, AttributeValue>();
        attributes.SetDouble("value", null, omitNullStrings: true);
        Assert.False(attributes.ContainsKey("value"));
    }

    #endregion

    #region SetDecimal Tests

    [Fact]
    public void SetDecimal_SetsValue()
    {
        var attributes = new Dictionary<string, AttributeValue>();
        attributes.SetDecimal("price", 99.99m);
        Assert.Equal("99.99", attributes["price"].N);
    }

    [Fact]
    public void SetDecimal_UsesInvariantCulture()
    {
        var attributes = new Dictionary<string, AttributeValue>();
        attributes.SetDecimal("amount", 1234.56m);
        Assert.Equal("1234.56", attributes["amount"].N);
        Assert.Contains(".", attributes["amount"].N);
    }

    [Fact]
    public void SetDecimal_SetsZero()
    {
        var attributes = new Dictionary<string, AttributeValue>();
        attributes.SetDecimal("price", 0m);
        Assert.Equal("0", attributes["price"].N);
    }

    [Fact]
    public void SetDecimal_SetsNegative()
    {
        var attributes = new Dictionary<string, AttributeValue>();
        attributes.SetDecimal("adj", -50.25m);
        Assert.Equal("-50.25", attributes["adj"].N);
    }

    [Fact]
    public void SetDecimal_SetsDynamoDBNull_WhenNullAndOmitFalse()
    {
        var attributes = new Dictionary<string, AttributeValue>();
        attributes.SetDecimal("price", null, omitNullStrings: false);
        Assert.True(attributes["price"].NULL);
    }

    [Fact]
    public void SetDecimal_OmitsAttribute_WhenNullAndOmitTrue()
    {
        var attributes = new Dictionary<string, AttributeValue>();
        attributes.SetDecimal("price", null, omitNullStrings: true);
        Assert.False(attributes.ContainsKey("price"));
    }

    #endregion

    #region DynamoDB NULL and Requiredness Tests

    [Fact]
    public void GetInt_ReturnsZero_WhenAttributeHasDynamoDBNull()
    {
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["count"] = new() { NULL = true },
        };
        Assert.Equal(0, attributes.GetInt("count", Requiredness.Optional));
    }

    [Fact]
    public void GetNullableInt_ReturnsNull_WhenAttributeHasDynamoDBNull()
    {
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["count"] = new() { NULL = true },
        };
        Assert.Null(attributes.GetNullableInt("count"));
    }

    [Fact]
    public void GetNullableDecimal_ReturnsNull_WhenAttributeHasDynamoDBNull_New()
    {
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["price"] = new() { NULL = true },
        };
        Assert.Null(attributes.GetNullableDecimal("price"));
    }

    [Fact]
    public void GetInt_ThrowsException_WhenKeyMissingAndRequired()
    {
        var attributes = new Dictionary<string, AttributeValue>();
        var ex = Assert.Throws<InvalidOperationException>(() =>
            attributes.GetInt("missing", Requiredness.Required)
        );
        Assert.Contains("does not contain an attribute named 'missing'", ex.Message);
    }

    [Fact]
    public void GetNullableFloat_ReturnsNull_WhenKeyMissingAndOptional()
    {
        var attributes = new Dictionary<string, AttributeValue>();
        Assert.Null(attributes.GetNullableFloat("missing", Requiredness.Optional));
    }

    #endregion

    #region Multi-Type Fluent Chaining

    [Fact]
    public void SetNumeric_AllowsFluentChaining_WithMixedTypes()
    {
        var attributes = new Dictionary<string, AttributeValue>();
        attributes
            .SetInt("count", 1)
            .SetLong("id", 2L)
            .SetFloat("ratio", 3.0f)
            .SetDouble("pi", 4.0)
            .SetDecimal("price", 5.5m);
        Assert.Equal(5, attributes.Count);
        Assert.Equal("1", attributes["count"].N);
        Assert.Equal("2", attributes["id"].N);
        Assert.Equal("3", attributes["ratio"].N);
        Assert.Equal("4", attributes["pi"].N);
        Assert.Equal("5.5", attributes["price"].N);
    }

    #endregion
}
