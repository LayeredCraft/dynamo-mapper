using Amazon.DynamoDBv2.Model;
using DynamoMapper.Runtime;

namespace DynamoMapper.Nested;

/// <summary>
/// Example: Nullable nested object - Warranty can be null
/// </summary>
public record Product
{
    public required string Sku { get; set; }
    public required string Name { get; set; }
    public Warranty? Warranty { get; set; }  // Nullable nested object
}

public record Warranty
{
    public int DurationMonths { get; set; }
    public required string Provider { get; set; }
}

[DynamoMapper]
public static partial class ProductMapper
{
    public static partial Dictionary<string, AttributeValue> ToItem(Product source);

    public static partial Product FromItem(Dictionary<string, AttributeValue> item);
}
