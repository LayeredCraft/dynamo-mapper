using System.IO;
using Amazon.DynamoDBv2.Model;
using DynamoMapper.Runtime;
using LayeredCraft.DynamoMapper.TestKit.Attributes;

namespace LayeredCraft.DynamoMapper.Runtime.Tests;

public class AttributeValueExtensionsCollectionTests
{
    // ==================== LIST TESTS ====================

    [Fact]
    public void GetList_ReturnsEmptyList_WhenKeyDoesNotExist()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        var result = attributes.GetList<string>("tags", Requiredness.Optional);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetList_ReturnsEmptyList_WhenValueIsNull()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["tags"] = new() { NULL = true }
        };

        // Act
        var result = attributes.GetList<string>("tags");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetList_ReturnsEmptyList_WhenListIsEmpty()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["tags"] = new() { L = new List<AttributeValue>() }
        };

        // Act
        var result = attributes.GetList<string>("tags");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetList_ReturnsListOfStrings_WhenListContainsValues()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["tags"] = new()
            {
                L = new List<AttributeValue>
                {
                    new() { S = "tag1" },
                    new() { S = "tag2" },
                    new() { S = "tag3" }
                }
            }
        };

        // Act
        var result = attributes.GetList<string>("tags");

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("tag1", result[0]);
        Assert.Equal("tag2", result[1]);
        Assert.Equal("tag3", result[2]);
    }

    [Fact]
    public void GetList_ReturnsListOfInts_WhenListContainsNumbers()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["scores"] = new()
            {
                L = new List<AttributeValue>
                {
                    new() { N = "100" },
                    new() { N = "200" },
                    new() { N = "300" }
                }
            }
        };

        // Act
        var result = attributes.GetList<int>("scores");

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(100, result[0]);
        Assert.Equal(200, result[1]);
        Assert.Equal(300, result[2]);
    }

    [Fact]
    public void GetList_ReturnsListOfBinary_WhenListContainsBinaryValues()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["payloads"] = new()
            {
                L = new List<AttributeValue>
                {
                    new() { B = new MemoryStream(new byte[] { 1, 2, 3 }) },
                    new() { B = new MemoryStream(new byte[] { 4, 5, 6 }) }
                }
            }
        };

        // Act
        var result = attributes.GetList<byte[]>("payloads");

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(new byte[] { 1, 2, 3 }, result[0]);
        Assert.Equal(new byte[] { 4, 5, 6 }, result[1]);
    }

    [Fact]
    public void GetList_HandlesNullableElements_WithNullValues()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["scores"] = new()
            {
                L = new List<AttributeValue>
                {
                    new() { N = "100" },
                    new() { NULL = true },
                    new() { N = "300" }
                }
            }
        };

        // Act
        var result = attributes.GetList<int?>("scores");

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(100, result[0]);
        Assert.Null(result[1]);
        Assert.Equal(300, result[2]);
    }

    [Fact]
    public void GetNullableList_ReturnsNull_WhenKeyDoesNotExist()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        var result = attributes.GetNullableList<string>("tags");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetNullableList_ReturnsNull_WhenValueIsNull()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["tags"] = new() { NULL = true }
        };

        // Act
        var result = attributes.GetNullableList<string>("tags");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void SetList_SetsListValue_WhenNonNullList()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();
        var tags = new List<string> { "tag1", "tag2", "tag3" };

        // Act
        attributes.SetList("tags", tags);

        // Assert
        Assert.True(attributes.ContainsKey("tags"));
        Assert.NotNull(attributes["tags"].L);
        Assert.Equal(3, attributes["tags"].L.Count);
        Assert.Equal("tag1", attributes["tags"].L[0].S);
        Assert.Equal("tag2", attributes["tags"].L[1].S);
        Assert.Equal("tag3", attributes["tags"].L[2].S);
    }

    [Fact]
    public void SetList_SetsEmptyList_WhenEmptyListAndOmitEmptyStringsFalse()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();
        var tags = new List<string>();

        // Act
        attributes.SetList("tags", tags, omitEmptyStrings: false);

        // Assert
        Assert.True(attributes.ContainsKey("tags"));
        Assert.NotNull(attributes["tags"].L);
        Assert.Empty(attributes["tags"].L);
    }

    [Fact]
    public void SetList_OmitsAttribute_WhenEmptyListAndOmitEmptyStringsTrue()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();
        var tags = new List<string>();

        // Act
        attributes.SetList("tags", tags, omitEmptyStrings: true);

        // Assert
        Assert.False(attributes.ContainsKey("tags"));
    }

    [Fact]
    public void SetList_SetsDynamoDBNull_WhenNullListAndOmitNullStringsFalse()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        attributes.SetList<string>("tags", null, omitNullStrings: false);

        // Assert
        Assert.True(attributes.ContainsKey("tags"));
        Assert.True(attributes["tags"].NULL);
    }

    [Fact]
    public void SetList_OmitsAttribute_WhenNullListAndOmitNullStringsTrue()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        attributes.SetList<string>("tags", null, omitNullStrings: true);

        // Assert
        Assert.False(attributes.ContainsKey("tags"));
    }

    [Fact]
    public void SetList_HandlesNullableElements_WithNullValues()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();
        var scores = new List<int?> { 100, null, 300 };

        // Act
        attributes.SetList("scores", scores);

        // Assert
        Assert.True(attributes.ContainsKey("scores"));
        Assert.Equal(3, attributes["scores"].L.Count);
        Assert.Equal("100", attributes["scores"].L[0].N);
        Assert.True(attributes["scores"].L[1].NULL);
        Assert.Equal("300", attributes["scores"].L[2].N);
    }

    [Fact]
    public void List_RoundTrip_WithStrings()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();
        var original = new List<string> { "alpha", "beta", "gamma" };

        // Act
        attributes.SetList("tags", original);
        var result = attributes.GetList<string>("tags");

        // Assert
        Assert.Equal(original, result);
    }

    [Fact]
    public void List_RoundTrip_WithInts()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();
        var original = new List<int> { 10, 20, 30, 40 };

        // Act
        attributes.SetList("numbers", original);
        var result = attributes.GetList<int>("numbers");

        // Assert
        Assert.Equal(original, result);
    }

    [Fact]
    public void List_RoundTrip_WithBinary()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();
        var original = new List<byte[]>
        {
            new byte[] { 0, 1, 2 },
            new byte[] { 3, 4, 5 }
        };

        // Act
        attributes.SetList("payloads", original);
        var result = attributes.GetList<byte[]>("payloads");

        // Assert
        Assert.Equal(original.Count, result.Count);
        Assert.Equal(original[0], result[0]);
        Assert.Equal(original[1], result[1]);
    }

    // ==================== MAP TESTS ====================

    [Fact]
    public void GetMap_ReturnsEmptyDictionary_WhenKeyDoesNotExist()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        var result = attributes.GetMap<int>("metadata", Requiredness.Optional);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetMap_ReturnsEmptyDictionary_WhenValueIsNull()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["metadata"] = new() { NULL = true }
        };

        // Act
        var result = attributes.GetMap<int>("metadata");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetMap_ReturnsDictionary_WhenMapContainsValues()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["metadata"] = new()
            {
                M = new Dictionary<string, AttributeValue>
                {
                    ["count"] = new() { N = "42" },
                    ["version"] = new() { N = "2" }
                }
            }
        };

        // Act
        var result = attributes.GetMap<int>("metadata");

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(42, result["count"]);
        Assert.Equal(2, result["version"]);
    }

    [Fact]
    public void GetNullableMap_ReturnsNull_WhenKeyDoesNotExist()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        var result = attributes.GetNullableMap<string>("attrs");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void SetMap_SetsMapValue_WhenNonNullDictionary()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();
        var metadata = new Dictionary<string, int> { ["count"] = 42, ["version"] = 2 };

        // Act
        attributes.SetMap("metadata", metadata);

        // Assert
        Assert.True(attributes.ContainsKey("metadata"));
        Assert.NotNull(attributes["metadata"].M);
        Assert.Equal(2, attributes["metadata"].M.Count);
        Assert.Equal("42", attributes["metadata"].M["count"].N);
        Assert.Equal("2", attributes["metadata"].M["version"].N);
    }

    [Fact]
    public void SetMap_SetsEmptyMap_WhenEmptyDictionaryAndOmitEmptyStringsFalse()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();
        var metadata = new Dictionary<string, int>();

        // Act
        attributes.SetMap("metadata", metadata, omitEmptyStrings: false);

        // Assert
        Assert.True(attributes.ContainsKey("metadata"));
        Assert.NotNull(attributes["metadata"].M);
        Assert.Empty(attributes["metadata"].M);
    }

    [Fact]
    public void SetMap_OmitsAttribute_WhenEmptyDictionaryAndOmitEmptyStringsTrue()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();
        var metadata = new Dictionary<string, int>();

        // Act
        attributes.SetMap("metadata", metadata, omitEmptyStrings: true);

        // Assert
        Assert.False(attributes.ContainsKey("metadata"));
    }

    [Fact]
    public void Map_RoundTrip_WithInts()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();
        var original = new Dictionary<string, int> { ["count"] = 42, ["version"] = 2, ["retries"] = 3 };

        // Act
        attributes.SetMap("metadata", original);
        var result = attributes.GetMap<int>("metadata");

        // Assert
        Assert.Equal(original, result);
    }

    [Fact]
    public void Map_RoundTrip_WithStrings()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();
        var original = new Dictionary<string, string>
        {
            ["name"] = "John",
            ["city"] = "Seattle",
            ["country"] = "USA"
        };

        // Act
        attributes.SetMap("attrs", original);
        var result = attributes.GetMap<string>("attrs");

        // Assert
        Assert.Equal(original, result);
    }


    // ==================== STRING SET TESTS ====================

    [Fact]
    public void GetStringSet_ReturnsEmptySet_WhenKeyDoesNotExist()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        var result = attributes.GetStringSet("categories", Requiredness.Optional);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetStringSet_ReturnsEmptySet_WhenValueIsNull()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["categories"] = new() { NULL = true }
        };

        // Act
        var result = attributes.GetStringSet("categories");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetStringSet_ReturnsHashSet_WhenSetContainsValues()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["categories"] = new()
            {
                SS = new List<string> { "electronics", "computers", "laptops" }
            }
        };

        // Act
        var result = attributes.GetStringSet("categories");

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Contains("electronics", result);
        Assert.Contains("computers", result);
        Assert.Contains("laptops", result);
    }

    [Fact]
    public void GetNullableStringSet_ReturnsNull_WhenKeyDoesNotExist()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        var result = attributes.GetNullableStringSet("categories");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void SetStringSet_SetsStringSet_WhenNonNullSet()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();
        var categories = new List<string> { "electronics", "computers", "laptops" };

        // Act
        attributes.SetStringSet("categories", categories);

        // Assert
        Assert.True(attributes.ContainsKey("categories"));
        Assert.NotNull(attributes["categories"].SS);
        Assert.Equal(3, attributes["categories"].SS.Count);
    }

    [Fact]
    public void SetStringSet_DeduplicatesValues()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();
        var categories = new List<string> { "electronics", "computers", "electronics", "laptops" };

        // Act
        attributes.SetStringSet("categories", categories);

        // Assert
        Assert.True(attributes.ContainsKey("categories"));
        Assert.Equal(3, attributes["categories"].SS.Count);
    }

    [Fact]
    public void SetStringSet_OmitsAttribute_WhenEmptySet()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();
        var categories = new List<string>();

        // Act
        attributes.SetStringSet("categories", categories);

        // Assert - DynamoDB does NOT allow empty sets
        Assert.False(attributes.ContainsKey("categories"));
    }

    [Fact]
    public void SetStringSet_FiltersNullValues()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();
        var categories = new List<string?> { "electronics", null, "computers" };

        // Act
        attributes.SetStringSet("categories", categories!);

        // Assert
        Assert.True(attributes.ContainsKey("categories"));
        Assert.Equal(2, attributes["categories"].SS.Count);
        Assert.Contains("electronics", attributes["categories"].SS);
        Assert.Contains("computers", attributes["categories"].SS);
    }

    [Fact]
    public void StringSet_RoundTrip()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();
        var original = new HashSet<string> { "electronics", "computers", "laptops" };

        // Act
        attributes.SetStringSet("categories", original);
        var result = attributes.GetStringSet("categories");

        // Assert
        Assert.Equal(original, result);
    }

    // ==================== NUMBER SET TESTS ====================

    [Fact]
    public void GetNumberSet_ReturnsEmptySet_WhenKeyDoesNotExist()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        var result = attributes.GetNumberSet<int>("numbers", Requiredness.Optional);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetNumberSet_ReturnsHashSet_WhenSetContainsInts()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["numbers"] = new()
            {
                NS = new List<string> { "1", "2", "3", "5", "8" }
            }
        };

        // Act
        var result = attributes.GetNumberSet<int>("numbers");

        // Assert
        Assert.Equal(5, result.Count);
        Assert.Contains(1, result);
        Assert.Contains(2, result);
        Assert.Contains(3, result);
        Assert.Contains(5, result);
        Assert.Contains(8, result);
    }

    [Fact]
    public void GetNumberSet_ReturnsHashSet_WhenSetContainsDoubles()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["prices"] = new()
            {
                NS = new List<string> { "19.99", "29.99", "39.99" }
            }
        };

        // Act
        var result = attributes.GetNumberSet<double>("prices");

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Contains(19.99, result);
        Assert.Contains(29.99, result);
        Assert.Contains(39.99, result);
    }

    [Fact]
    public void SetNumberSet_SetsNumberSet_WhenNonNullSet()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();
        var numbers = new List<int> { 1, 2, 3, 5, 8 };

        // Act
        attributes.SetNumberSet("numbers", numbers);

        // Assert
        Assert.True(attributes.ContainsKey("numbers"));
        Assert.NotNull(attributes["numbers"].NS);
        Assert.Equal(5, attributes["numbers"].NS.Count);
    }

    [Fact]
    public void SetNumberSet_DeduplicatesValues()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();
        var numbers = new List<int> { 1, 2, 3, 2, 5, 1 };

        // Act
        attributes.SetNumberSet("numbers", numbers);

        // Assert
        Assert.True(attributes.ContainsKey("numbers"));
        Assert.Equal(4, attributes["numbers"].NS.Count);
    }

    [Fact]
    public void SetNumberSet_OmitsAttribute_WhenEmptySet()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();
        var numbers = new List<int>();

        // Act
        attributes.SetNumberSet("numbers", numbers);

        // Assert - DynamoDB does NOT allow empty sets
        Assert.False(attributes.ContainsKey("numbers"));
    }

    [Fact]
    public void NumberSet_RoundTrip_WithInts()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();
        var original = new HashSet<int> { 1, 2, 3, 5, 8, 13 };

        // Act
        attributes.SetNumberSet("numbers", original);
        var result = attributes.GetNumberSet<int>("numbers");

        // Assert
        Assert.Equal(original, result);
    }

    [Fact]
    public void NumberSet_RoundTrip_WithDecimals()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();
        var original = new HashSet<decimal> { 10.5m, 20.25m, 30.75m };

        // Act
        attributes.SetNumberSet("prices", original);
        var result = attributes.GetNumberSet<decimal>("prices");

        // Assert
        Assert.Equal(original, result);
    }

    // ==================== BINARY SET TESTS ====================

    [Fact]
    public void GetBinarySet_ReturnsEmptySet_WhenKeyDoesNotExist()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        var result = attributes.GetBinarySet("payloads", Requiredness.Optional);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetBinarySet_ReturnsEmptySet_WhenValueIsNull()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["payloads"] = new() { NULL = true }
        };

        // Act
        var result = attributes.GetBinarySet("payloads");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetBinarySet_ReturnsHashSet_WhenSetContainsValues()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["payloads"] = new()
            {
                BS = new List<MemoryStream>
                {
                    new(new byte[] { 1, 2, 3 }),
                    new(new byte[] { 4, 5, 6 })
                }
            }
        };

        // Act
        var result = attributes.GetBinarySet("payloads");

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, bytes => bytes.SequenceEqual(new byte[] { 1, 2, 3 }));
        Assert.Contains(result, bytes => bytes.SequenceEqual(new byte[] { 4, 5, 6 }));
    }

    [Fact]
    public void GetNullableBinarySet_ReturnsNull_WhenValueIsNull()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["payloads"] = new() { NULL = true }
        };

        // Act
        var result = attributes.GetNullableBinarySet("payloads");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void SetBinarySet_SetsBinarySet_WhenNonNullSet()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();
        var payloads = new List<byte[]>
        {
            new byte[] { 1, 2, 3 },
            new byte[] { 4, 5, 6 }
        };

        // Act
        attributes.SetBinarySet("payloads", payloads);

        // Assert
        Assert.True(attributes.ContainsKey("payloads"));
        Assert.NotNull(attributes["payloads"].BS);
        Assert.Equal(2, attributes["payloads"].BS.Count);
    }

    [Fact]
    public void SetBinarySet_DeduplicatesValues()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();
        var payloads = new List<byte[]>
        {
            new byte[] { 1, 2, 3 },
            new byte[] { 1, 2, 3 }
        };

        // Act
        attributes.SetBinarySet("payloads", payloads);

        // Assert
        Assert.True(attributes.ContainsKey("payloads"));
        Assert.Equal(1, attributes["payloads"].BS.Count);
    }

    [Fact]
    public void SetBinarySet_OmitsAttribute_WhenEmptySet()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();
        var payloads = new List<byte[]>();

        // Act
        attributes.SetBinarySet("payloads", payloads);

        // Assert - DynamoDB does NOT allow empty sets
        Assert.False(attributes.ContainsKey("payloads"));
    }

    [Fact]
    public void BinarySet_RoundTrip()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();
        var original = new List<byte[]>
        {
            new byte[] { 7, 8, 9 },
            new byte[] { 10, 11, 12 }
        };
        var expected = original
            .Select(bytes => string.Join(",", bytes))
            .ToHashSet(StringComparer.Ordinal);

        // Act
        attributes.SetBinarySet("payloads", original);
        var result = attributes.GetBinarySet("payloads");
        var actual = result
            .Select(bytes => string.Join(",", bytes))
            .ToHashSet(StringComparer.Ordinal);

        // Assert
        Assert.Equal(expected, actual);
    }

    // ==================== FLUENT CHAINING TESTS ====================

    [Fact]
    public void Collections_SupportFluentChaining()
    {
        // Arrange
        var attributes = new Dictionary<string, AttributeValue>();

        // Act
        attributes
            .SetList("tags", new List<string> { "tag1", "tag2" })
            .SetMap("metadata", new Dictionary<string, int> { ["count"] = 42 })
            .SetStringSet("categories", new List<string> { "cat1", "cat2" })
            .SetNumberSet("numbers", new List<int> { 1, 2, 3 })
            .SetBinarySet("payloads", new List<byte[]> { new byte[] { 1, 2, 3 } });

        // Assert
        Assert.Equal(5, attributes.Count);
        Assert.True(attributes.ContainsKey("tags"));
        Assert.True(attributes.ContainsKey("metadata"));
        Assert.True(attributes.ContainsKey("categories"));
        Assert.True(attributes.ContainsKey("numbers"));
        Assert.True(attributes.ContainsKey("payloads"));
    }
}
