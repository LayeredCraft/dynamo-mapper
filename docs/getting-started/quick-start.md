# Quick Start

Build your first DynamoMapper in 5 minutes.

## Step 1: Create a Domain Model

```csharp
public class Order
{
    public string CustomerId { get; set; }
    public Guid OrderId { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Notes { get; set; }
}

public enum OrderStatus
{
    Pending,
    Confirmed,
    Shipped,
    Delivered
}
```

## Step 2: Create a Mapper

```csharp
using DynamoMapper.Attributes;
using Amazon.DynamoDBv2.Model;

namespace MyApp.Data;

[DynamoMapper(Convention = DynamoNamingConvention.CamelCase)]
public static partial class OrderMapper
{
    [DynamoField(nameof(Order.Notes), OmitIfNullOrWhiteSpace = true)]
    public static partial Dictionary<string, AttributeValue> ToItem(Order source);

    public static partial Order FromItem(Dictionary<string, AttributeValue> item);
}
```

## Step 3: Use the Mapper

```csharp
var order = new Order
{
    CustomerId = "customer-123",
    OrderId = Guid.NewGuid(),
    TotalAmount = 149.99m,
    Status = OrderStatus.Pending,
    CreatedAt = DateTime.UtcNow
};

// Convert to DynamoDB item
var item = OrderMapper.ToItem(order);

// Save to DynamoDB
await dynamoDbClient.PutItemAsync(new PutItemRequest
{
    TableName = "Orders",
    Item = item
});

// Retrieve from DynamoDB
var response = await dynamoDbClient.GetItemAsync(new GetItemRequest
{
    TableName = "Orders",
    Key = new Dictionary<string, AttributeValue>
    {
        ["customerId"] = new AttributeValue { S = "customer-123" },
        ["orderId"] = new AttributeValue { S = order.OrderId.ToString() }
    }
});

// Convert back to domain model
var retrievedOrder = OrderMapper.FromItem(response.Item);
```

## What Just Happened?

1. **`[DynamoMapper]`** - Marks the mapper and sets CamelCase naming (OrderId → orderId)
2. **`[DynamoField]`** - Configured Notes field to omit if null/whitespace
3. **Generated Code** - DynamoMapper generated ToItem and FromItem implementations at compile time

## Next Steps

- [Basic Mapping](../usage/basic-mapping.md) - Learn core mapping concepts
- [Field Configuration](../usage/field-configuration.md) - Configure fields in detail
- [Single-Table Design](../examples/single-table-design.md) - See single-table patterns
