---
title: Single Table Design
description: Using DynamoMapper for DynamoDB single-table design patterns
---

# Single Table Design

Single-table design is a DynamoDB best practice that stores multiple entity types in a single table using composite keys. DynamoMapper's customization hooks make implementing single-table patterns straightforward and type-safe.

## Why Single Table Design?

Single-table design offers several advantages:

- **Cost efficiency** - Fewer tables to provision
- **Atomic transactions** - Multiple entities in one transaction
- **Query flexibility** - GSIs enable diverse access patterns
- **Scalability** - Better partition distribution

DynamoMapper's hooks enable these patterns without polluting domain models with persistence concerns.

## Basic Single-Table Pattern

### Domain Models

Clean domain models without DynamoDB concerns:

```csharp
public class Customer
{
    public string CustomerId { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class Order
{
    public Guid OrderId { get; set; }
    public string CustomerId { get; set; }
    public OrderStatus Status { get; set; }
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### Customer Mapper

```csharp
using Amazon.DynamoDBv2.Model;

[DynamoMapper(Convention = DynamoNamingConvention.CamelCase)]
public static partial class CustomerMapper
{
    public static partial Dictionary<string, AttributeValue> FromModel(Customer source);
    public static partial Customer ToModel(Dictionary<string, AttributeValue> item);

    static partial void AfterFromModel(Customer source, Dictionary<string, AttributeValue> item)
    {
        // PK/SK for customer entity
        item["pk"] = new AttributeValue { S = $"CUSTOMER#{source.CustomerId}" };
        item["sk"] = new AttributeValue { S = "METADATA" };

        // Entity type for polymorphic queries
        item["entityType"] = new AttributeValue { S = "Customer" };
        item["recordType"] = new AttributeValue { S = "Customer" };

        // GSI1: Query customers by email
        item["gsi1pk"] = new AttributeValue { S = $"EMAIL#{source.Email}" };
        item["gsi1sk"] = new AttributeValue { S = $"CUSTOMER#{source.CustomerId}" };
    }

    static partial void BeforeToModel(Dictionary<string, AttributeValue> item)
    {
        // Validate entity type before mapping
        if (item.TryGetValue("entityType", out var typeAttr) && typeAttr.S != "Customer")
        {
            throw new DynamoMappingException(
                targetType: "Customer",
                details: $"Cannot deserialize {typeAttr.S} as Customer");
        }
    }
}
```

### Order Mapper

```csharp
[DynamoMapper(Convention = DynamoNamingConvention.CamelCase)]
public static partial class OrderMapper
{
    [DynamoField(nameof(Order.Status), Converter = typeof(OrderStatusConverter))]
    public static partial Dictionary<string, AttributeValue> FromModel(Order source);

    public static partial Order ToModel(Dictionary<string, AttributeValue> item);

    static partial void AfterFromModel(Order source, Dictionary<string, AttributeValue> item)
    {
        // PK/SK for order under customer
        item["pk"] = new AttributeValue { S = $"CUSTOMER#{source.CustomerId}" };
        item["sk"] = new AttributeValue { S = $"ORDER#{source.OrderId}#{source.CreatedAt:yyyy-MM-dd}" };

        // Entity type discrimination
        item["entityType"] = new AttributeValue { S = "Order" };
        item["recordType"] = new AttributeValue { S = "Order" };

        // GSI1: Query orders by status
        item["gsi1pk"] = new AttributeValue { S = $"STATUS#{source.Status.Name}" };
        item["gsi1sk"] = new AttributeValue { S = source.CreatedAt.ToString("O") };

        // GSI2: Query all orders by date
        item["gsi2pk"] = new AttributeValue { S = "ORDER" };
        item["gsi2sk"] = new AttributeValue { S = source.CreatedAt.ToString("O") };
    }

    static partial void BeforeToModel(Dictionary<string, AttributeValue> item)
    {
        if (item.TryGetValue("entityType", out var typeAttr) && typeAttr.S != "Order")
        {
            throw new DynamoMappingException(
                targetType: "Order",
                details: $"Cannot deserialize {typeAttr.S} as Order");
        }
    }
}
```

## Table Structure

### Base Table

| PK | SK | entityType | Attributes |
|----|----|-----------|-----------|
| CUSTOMER#C1 | METADATA | Customer | email, name, createdAt |
| CUSTOMER#C1 | ORDER#O1#2024-01-15 | Order | orderId, status, total, createdAt |
| CUSTOMER#C1 | ORDER#O2#2024-02-20 | Order | orderId, status, total, createdAt |

### GSI1: Email Lookup / Status Queries

| gsi1pk | gsi1sk | entityType |
|--------|--------|-----------|
| EMAIL#user@example.com | CUSTOMER#C1 | Customer |
| STATUS#Pending | 2024-01-15T10:30:00Z | Order |
| STATUS#Shipped | 2024-02-20T14:45:00Z | Order |

### GSI2: All Orders by Date

| gsi2pk | gsi2sk | entityType |
|--------|--------|-----------|
| ORDER | 2024-01-15T10:30:00Z | Order |
| ORDER | 2024-02-20T14:45:00Z | Order |

## Access Patterns

### 1. Get Customer by ID

```csharp
var request = new GetItemRequest
{
    TableName = "AppTable",
    Key = new Dictionary<string, AttributeValue>
    {
        ["pk"] = new AttributeValue { S = $"CUSTOMER#{customerId}" },
        ["sk"] = new AttributeValue { S = "METADATA" }
    }
};

var response = await dynamoDb.GetItemAsync(request);
var customer = CustomerMapper.ToModel(response.Item);
```

### 2. Get All Orders for Customer

```csharp
var request = new QueryRequest
{
    TableName = "AppTable",
    KeyConditionExpression = "pk = :pk AND begins_with(sk, :sk)",
    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
    {
        [":pk"] = new AttributeValue { S = $"CUSTOMER#{customerId}" },
        [":sk"] = new AttributeValue { S = "ORDER#" }
    }
};

var response = await dynamoDb.QueryAsync(request);
var orders = response.Items.Select(OrderMapper.ToModel).ToList();
```

### 3. Get Customer by Email (GSI1)

```csharp
var request = new QueryRequest
{
    TableName = "AppTable",
    IndexName = "GSI1",
    KeyConditionExpression = "gsi1pk = :email",
    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
    {
        [":email"] = new AttributeValue { S = $"EMAIL#{email}" }
    }
};

var response = await dynamoDb.QueryAsync(request);
var customer = CustomerMapper.ToModel(response.Items.First());
```

### 4. Get Orders by Status (GSI1)

```csharp
var request = new QueryRequest
{
    TableName = "AppTable",
    IndexName = "GSI1",
    KeyConditionExpression = "gsi1pk = :status",
    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
    {
        [":status"] = new AttributeValue { S = $"STATUS#{status}" }
    }
};

var response = await dynamoDb.QueryAsync(request);
var orders = response.Items.Select(OrderMapper.ToModel).ToList();
```

## Advanced Patterns

### Composite Entity with TTL

```csharp
public class Session
{
    public Guid SessionId { get; set; }
    public string UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}

[DynamoMapper(Convention = DynamoNamingConvention.CamelCase)]
public static partial class SessionMapper
{
    public static partial Dictionary<string, AttributeValue> FromModel(Session source);
    public static partial Session ToModel(Dictionary<string, AttributeValue> item);

    static partial void AfterFromModel(Session source, Dictionary<string, AttributeValue> item)
    {
        // Session keys
        item["pk"] = new AttributeValue { S = $"USER#{source.UserId}" };
        item["sk"] = new AttributeValue { S = $"SESSION#{source.SessionId}" };
        item["entityType"] = new AttributeValue { S = "Session" };

        // TTL for automatic deletion
        var ttl = new DateTimeOffset(source.ExpiresAt).ToUnixTimeSeconds();
        item["ttl"] = new AttributeValue { N = ttl.ToString() };

        // GSI1: Query active sessions
        if (source.ExpiresAt > DateTime.UtcNow)
        {
            item["gsi1pk"] = new AttributeValue { S = "ACTIVE_SESSION" };
            item["gsi1sk"] = new AttributeValue { S = source.ExpiresAt.ToString("O") };
        }
    }
}
```

### Hierarchical Data

```csharp
public class Product
{
    public Guid ProductId { get; set; }
    public string CategoryId { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}

public class ProductReview
{
    public Guid ReviewId { get; set; }
    public Guid ProductId { get; set; }
    public string UserId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}

[DynamoMapper(Convention = DynamoNamingConvention.CamelCase)]
public static partial class ProductMapper
{
    public static partial Dictionary<string, AttributeValue> FromModel(Product source);
    public static partial Product ToModel(Dictionary<string, AttributeValue> item);

    static partial void AfterFromModel(Product source, Dictionary<string, AttributeValue> item)
    {
        // Product hierarchy under category
        item["pk"] = new AttributeValue { S = $"CATEGORY#{source.CategoryId}" };
        item["sk"] = new AttributeValue { S = $"PRODUCT#{source.ProductId}" };
        item["entityType"] = new AttributeValue { S = "Product" };

        // GSI1: All products by price
        item["gsi1pk"] = new AttributeValue { S = "PRODUCT" };
        item["gsi1sk"] = new AttributeValue { S = source.Price.ToString("0000000.00") };
    }
}

[DynamoMapper(Convention = DynamoNamingConvention.CamelCase)]
public static partial class ProductReviewMapper
{
    public static partial Dictionary<string, AttributeValue> FromModel(ProductReview source);
    public static partial ProductReview ToModel(Dictionary<string, AttributeValue> item);

    static partial void AfterFromModel(ProductReview source, Dictionary<string, AttributeValue> item)
    {
        // Reviews under product
        item["pk"] = new AttributeValue { S = $"PRODUCT#{source.ProductId}" };
        item["sk"] = new AttributeValue { S = $"REVIEW#{source.ReviewId}#{source.CreatedAt:yyyy-MM-dd}" };
        item["entityType"] = new AttributeValue { S = "ProductReview" };

        // GSI1: User's reviews
        item["gsi1pk"] = new AttributeValue { S = $"USER#{source.UserId}" };
        item["gsi1sk"] = new AttributeValue { S = source.CreatedAt.ToString("O") };

        // GSI2: Reviews by rating
        item["gsi2pk"] = new AttributeValue { S = "REVIEW" };
        item["gsi2sk"] = new AttributeValue { S = $"{source.Rating}#{source.CreatedAt:O}" };
    }
}
```

## Attribute Bag Pattern

For forward compatibility, capture unmapped attributes:

```csharp
public class Product
{
    public Guid ProductId { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }

    // Capture additional attributes for forward compatibility
    public Dictionary<string, AttributeValue>? Metadata { get; set; }
}

[DynamoMapper(Convention = DynamoNamingConvention.CamelCase)]
public static partial class ProductMapper
{
    [DynamoIgnore(nameof(Product.Metadata))]
    public static partial Dictionary<string, AttributeValue> FromModel(Product source);

    [DynamoIgnore(nameof(Product.Metadata))]
    public static partial Product ToModel(Dictionary<string, AttributeValue> item);

    static partial void AfterFromModel(Product source, Dictionary<string, AttributeValue> item)
    {
        // Single-table keys
        item["pk"] = new AttributeValue { S = $"PRODUCT#{source.ProductId}" };
        item["sk"] = new AttributeValue { S = "METADATA" };
        item["entityType"] = new AttributeValue { S = "Product" };

        // Merge metadata bag
        if (source.Metadata != null)
        {
            foreach (var kvp in source.Metadata)
            {
                item[kvp.Key] = kvp.Value;
            }
        }
    }

    static partial void AfterToModel(Dictionary<string, AttributeValue> item, ref Product entity)
    {
        // Capture unmapped attributes
        var coreAttributes = new HashSet<string>
        {
            "pk", "sk", "entityType", "recordType",
            "productId", "name", "price"
        };

        entity.Metadata = item
            .Where(kvp => !coreAttributes.Contains(kvp.Key))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }
}
```

## Conditional Keys

Keys can vary based on entity state:

```csharp
public class Product
{
    public Guid ProductId { get; set; }
    public string Name { get; set; }
    public bool IsArchived { get; set; }
    public DateTime? ArchivedAt { get; set; }
}

[DynamoMapper(Convention = DynamoNamingConvention.CamelCase)]
public static partial class ProductMapper
{
    public static partial Dictionary<string, AttributeValue> FromModel(Product source);
    public static partial Product ToModel(Dictionary<string, AttributeValue> item);

    static partial void AfterFromModel(Product source, Dictionary<string, AttributeValue> item)
    {
        // Base keys
        item["pk"] = new AttributeValue { S = $"PRODUCT#{source.ProductId}" };
        item["sk"] = new AttributeValue { S = "METADATA" };
        item["entityType"] = new AttributeValue { S = "Product" };

        // Conditional GSI for active vs archived
        if (source.IsArchived)
        {
            item["gsi1pk"] = new AttributeValue { S = "ARCHIVED_PRODUCT" };
            item["gsi1sk"] = new AttributeValue { S = source.ArchivedAt?.ToString("O") ?? DateTime.UtcNow.ToString("O") };
        }
        else
        {
            item["gsi1pk"] = new AttributeValue { S = "ACTIVE_PRODUCT" };
            item["gsi1sk"] = new AttributeValue { S = source.Name };
        }
    }
}
```

## Polymorphic Queries

Handle multiple entity types in a single query:

```csharp
public class QueryResult
{
    public string EntityType { get; set; }
    public Dictionary<string, AttributeValue> RawItem { get; set; }
}

// Query multiple entity types
var request = new QueryRequest
{
    TableName = "AppTable",
    KeyConditionExpression = "pk = :pk",
    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
    {
        [":pk"] = new AttributeValue { S = $"CUSTOMER#{customerId}" }
    }
};

var response = await dynamoDb.QueryAsync(request);

var results = response.Items.Select(item =>
{
    var entityType = item["entityType"].S;
    return entityType switch
    {
        "Customer" => (object)CustomerMapper.ToModel(item),
        "Order" => (object)OrderMapper.ToModel(item),
        _ => throw new InvalidOperationException($"Unknown entity type: {entityType}")
    };
}).ToList();
```

## Sparse Indexes

Use sparse GSIs for conditional attributes:

```csharp
public class Order
{
    public Guid OrderId { get; set; }
    public string CustomerId { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime? ShippedAt { get; set; }
    public string? TrackingNumber { get; set; }
}

[DynamoMapper(Convention = DynamoNamingConvention.CamelCase)]
public static partial class OrderMapper
{
    [DynamoField(nameof(Order.Status), Converter = typeof(OrderStatusConverter))]
    public static partial Dictionary<string, AttributeValue> FromModel(Order source);

    public static partial Order ToModel(Dictionary<string, AttributeValue> item);

    static partial void AfterFromModel(Order source, Dictionary<string, AttributeValue> item)
    {
        item["pk"] = new AttributeValue { S = $"CUSTOMER#{source.CustomerId}" };
        item["sk"] = new AttributeValue { S = $"ORDER#{source.OrderId}" };
        item["entityType"] = new AttributeValue { S = "Order" };

        // Sparse GSI: Only shipped orders
        if (source.ShippedAt.HasValue && !string.IsNullOrEmpty(source.TrackingNumber))
        {
            item["gsi1pk"] = new AttributeValue { S = "SHIPPED" };
            item["gsi1sk"] = new AttributeValue { S = source.ShippedAt.Value.ToString("O") };
        }
    }
}
```

## Best Practices

1. **Use EntityType for polymorphic queries**
   - Always include entity type discriminator
   - Validate on read to prevent mapping errors

2. **Design access patterns first**
   - Plan PK/SK and GSI keys around queries
   - Use hooks to generate keys from domain properties

3. **Keep domain models clean**
   - No PK/SK properties in domain models
   - Use hooks to add DynamoDB-specific attributes

4. **Use sparse indexes for conditional data**
   - Only populate GSI keys when needed
   - Reduces storage and improves query performance

5. **Plan for forward compatibility**
   - Use attribute bags to capture unmapped fields
   - Allows schema evolution without breaking changes

6. **Test your access patterns**
   - Verify queries return expected entities
   - Test entity type validation in hooks

7. **Document your key structure**
   - Clearly document PK/SK patterns
   - Include GSI usage in mapper comments

## See Also

- [Customization Hooks](../usage/customization-hooks.md) - Complete hook documentation
- [Phase 1 Requirements](../roadmap/phase-1.md#11-customization-hooks-phase-1) - Hook specifications
- [DynamoDB Single-Table Design](https://aws.amazon.com/blogs/compute/creating-a-single-table-design-with-amazon-dynamodb/) - AWS Best Practices