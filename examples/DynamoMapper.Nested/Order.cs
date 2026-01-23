using Amazon.DynamoDBv2.Model;
using DynamoMapper.Runtime;

namespace DynamoMapper.Nested;

public record Order
{
    public required string Id { get; set; }
    public required Address ShippingAddress { get; set; }
}

public record Address
{
    public required string Line1 { get; set; }
    public required string City { get; set; }
    public required string PostalCode { get; set; }
}

[DynamoMapper]
public static partial class OrderMapper
{
    public static partial Dictionary<string, AttributeValue> ToItem(Order source);

    public static partial Order FromItem(Dictionary<string, AttributeValue> item);
}
