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
            ["count"] = new AttributeValue { N = value.ToString() }
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
        var result = attributes.GetInt("count");

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void GetInt_ReturnsZero_WhenParseFailsInvalid()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["count"] = new AttributeValue { N = "invalid" }
        };

        // Act
        var result = attributes.GetInt("count");

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
            ["count"] = new AttributeValue { N = value.ToString() }
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
            ["bigNumber"] = new AttributeValue { N = value.ToString() }
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
        var result = attributes.GetLong("bigNumber");

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
            ["bigNumber"] = new AttributeValue { N = value.ToString() }
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
            ["ratio"] = new AttributeValue { N = value.ToString(System.Globalization.CultureInfo.InvariantCulture) }
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
        var result = attributes.GetFloat("ratio");

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
            ["ratio"] = new AttributeValue { N = value.ToString(System.Globalization.CultureInfo.InvariantCulture) }
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
            ["measurement"] = new AttributeValue { N = value.ToString(System.Globalization.CultureInfo.InvariantCulture) }
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
        var result = attributes.GetDouble("measurement");

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
            ["measurement"] = new AttributeValue { N = value.ToString(System.Globalization.CultureInfo.InvariantCulture) }
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
            ["price"] = new AttributeValue { N = value.ToString(System.Globalization.CultureInfo.InvariantCulture) }
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
        var result = attributes.GetDecimal("price");

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
            ["price"] = new AttributeValue { N = value.ToString(System.Globalization.CultureInfo.InvariantCulture) }
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
}