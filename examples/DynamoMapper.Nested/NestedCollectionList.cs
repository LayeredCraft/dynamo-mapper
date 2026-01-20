using Amazon.DynamoDBv2.Model;
using DynamoMapper.Runtime;

namespace DynamoMapper.Nested;

/// <summary>
/// Example: List of nested objects (inline generation)
/// </summary>
public record ShoppingCart
{
    public string CartId { get; set; }
    public List<CartItem> Items { get; set; }
}

public record CartItem
{
    public string ProductId { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

[DynamoMapper]
public static partial class ShoppingCartMapper
{
    public static partial Dictionary<string, AttributeValue> ToItem(ShoppingCart source);

    public static partial ShoppingCart FromItem(Dictionary<string, AttributeValue> item);
}