---
title: Customization Hooks
description: Using partial methods to extend DynamoMapper mapping behavior
---

# Customization Hooks

Customization hooks are first-class extension points in DynamoMapper that allow you to inject custom logic into the mapping pipeline. Implemented as optional partial methods, hooks enable powerful patterns like single-table design, record discrimination, and attribute passthrough without sacrificing performance or type safety.

## Overview

DynamoMapper provides **four lifecycle hooks** that execute at strategic points during mapping:

| Hook              | Phase     | When                      | Use For                            |
|-------------------|-----------|---------------------------|------------------------------------|
| `BeforeFromModel` | FromModel | Before property mapping   | Pre-processing, initialization     |
| `AfterFromModel`  | FromModel | After property mapping    | PK/SK injection, TTL, metadata     |
| `BeforeToModel`   | ToModel   | Before property mapping   | Validation, preprocessing          |
| `AfterToModel`    | ToModel   | After object construction | Normalization, computed properties |

Hooks are implemented as **optional partial void methods**. If not implemented, they compile away with zero runtime overhead.

## Why Hooks?

DynamoMapper focuses exclusively on mapping properties to/from DynamoDB AttributeValue dictionaries. Hooks enable DynamoDB-specific patterns without bloating the core mapping logic:

- **Single-table design** - Add PK/SK, record types, GSI keys
- **TTL management** - Inject expiration timestamps
- **Attribute bags** - Capture unmapped attributes for forward compatibility
- **Post-hydration normalization** - Compute derived properties, normalize data
- **Audit and logging** - Track mapping operations

## Hook Signatures

All hooks must be declared as `static partial void` on the mapper class.

### BeforeFromModel

Invoked before property mapping during `FromModel`.

```csharp
static partial void BeforeFromModel(T source, Dictionary<string, AttributeValue> item);
```

**Parameters:**
- `source` - The domain object being mapped
- `item` - An empty dictionary that will be populated

**Use cases:**
- Initialize dictionary capacity
- Add fixed metadata before mapping
- Pre-compute derived values

### AfterFromModel

Invoked after property mapping during `FromModel`.

```csharp
static partial void AfterFromModel(T source, Dictionary<string, AttributeValue> item);
```

**Parameters:**
- `source` - The domain object (for reference)
- `item` - The fully populated dictionary with all mapped properties

**Use cases:**
- Add single-table keys (PK, SK)
- Inject discriminator fields
- Add TTL timestamps
- Merge additional attribute dictionaries
- Override generated mappings

### BeforeToModel

Invoked before property mapping during `ToModel`.

```csharp
static partial void BeforeToModel(Dictionary<string, AttributeValue> item);
```

**Parameters:**
- `item` - The raw DynamoDB item dictionary

**Use cases:**
- Validate required metadata
- Transform item structure
- Extract unmapped attributes
- Log or audit incoming data

### AfterToModel

Invoked after property mapping and object construction during `ToModel`.

```csharp
static partial void AfterToModel(Dictionary<string, AttributeValue> item, ref T entity);
```

**Parameters:**
- `item` - The raw DynamoDB item (for reference)
- `entity` - The constructed entity (passed by `ref` for modification)

**Use cases:**
- Post-construction normalization
- Populate record discriminators
- Hydrate computed properties
- Populate attribute bags
- Validate entity state

## Basic Usage

### Single File

```csharp
using Amazon.DynamoDBv2.Model;

[DynamoMapper(Convention = DynamoNamingConvention.CamelCase)]
public static partial class ProductMapper
{
    public static partial Dictionary<string, AttributeValue> FromModel(Product source);
    public static partial Product ToModel(Dictionary<string, AttributeValue> item);

    static partial void AfterFromModel(Product source, Dictionary<string, AttributeValue> item)
    {
        item["pk"] = new AttributeValue { S = $"PRODUCT#{source.ProductId}" };
        item["sk"] = new AttributeValue { S = "METADATA" };
        item["recordType"] = new AttributeValue { S = "Product" };
    }
}
```

### Separate File for Organization

For complex mappers, place hooks in a separate file:

```csharp
// ProductMapper.cs
[DynamoMapper(Convention = DynamoNamingConvention.CamelCase)]
public static partial class ProductMapper
{
    public static partial Dictionary<string, AttributeValue> FromModel(Product source);
    public static partial Product ToModel(Dictionary<string, AttributeValue> item);
}

// ProductMapper.Hooks.cs
public static partial class ProductMapper
{
    static partial void BeforeFromModel(Product source, Dictionary<string, AttributeValue> item)
    {
        // Pre-processing logic
    }

    static partial void AfterFromModel(Product source, Dictionary<string, AttributeValue> item)
    {
        item["pk"] = new AttributeValue { S = $"PRODUCT#{source.ProductId}" };
        item["sk"] = new AttributeValue { S = "METADATA" };
        item["recordType"] = new AttributeValue { S = "Product" };
    }

    static partial void AfterToModel(Dictionary<string, AttributeValue> item, ref Product entity)
    {
        // Post-hydration logic
    }
}
```

## Common Patterns

### Single-Table PK/SK Composition

The most common hook use case - adding partition and sort keys:

```csharp
public class Order
{
    public Guid OrderId { get; set; }
    public string CustomerId { get; set; }
    public OrderStatus Status { get; set; }
    public decimal Total { get; set; }
}

[DynamoMapper(Convention = DynamoNamingConvention.CamelCase)]
public static partial class OrderMapper
{
    public static partial Dictionary<string, AttributeValue> FromModel(Order source);
    public static partial Order ToModel(Dictionary<string, AttributeValue> item);

    static partial void AfterFromModel(Order source, Dictionary<string, AttributeValue> item)
    {
        // Composite keys for single-table design
        item["pk"] = new AttributeValue { S = $"CUSTOMER#{source.CustomerId}" };
        item["sk"] = new AttributeValue { S = $"ORDER#{source.OrderId}" };
        item["recordType"] = new AttributeValue { S = "Order" };
    }
}
```

### Record Type Discrimination

Validate entity types on read:

```csharp
[DynamoMapper(Convention = DynamoNamingConvention.CamelCase)]
public static partial class CustomerMapper
{
    public static partial Dictionary<string, AttributeValue> FromModel(Customer source);
    public static partial Customer ToModel(Dictionary<string, AttributeValue> item);

    static partial void AfterFromModel(Customer source, Dictionary<string, AttributeValue> item)
    {
        item["entityType"] = new AttributeValue { S = nameof(Customer) };
    }

    static partial void AfterToModel(Dictionary<string, AttributeValue> item, ref Customer entity)
    {
        if (item.TryGetValue("entityType", out var typeAttr))
        {
            if (typeAttr.S != nameof(Customer))
            {
                throw new DynamoMappingException(
                    targetType: nameof(Customer),
                    details: $"Expected {nameof(Customer)}, got {typeAttr.S}");
            }
        }
    }
}
```

### TTL (Time To Live) Attributes

Add expiration timestamps:

```csharp
public class Session
{
    public Guid SessionId { get; set; }
    public string UserId { get; set; }
    public DateTime CreatedAt { get; set; }
}

[DynamoMapper(Convention = DynamoNamingConvention.CamelCase)]
public static partial class SessionMapper
{
    public static partial Dictionary<string, AttributeValue> FromModel(Session source);
    public static partial Session ToModel(Dictionary<string, AttributeValue> item);

    static partial void AfterFromModel(Session source, Dictionary<string, AttributeValue> item)
    {
        // Set TTL to 24 hours from now
        var ttl = DateTimeOffset.UtcNow.AddHours(24).ToUnixTimeSeconds();
        item["ttl"] = new AttributeValue { N = ttl.ToString() };
    }
}
```

### Unmapped Attribute Bags

Capture attributes not explicitly mapped for forward compatibility:

```csharp
public class Product
{
    public Guid ProductId { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }

    // Bag for unmapped attributes
    public Dictionary<string, AttributeValue>? AdditionalAttributes { get; set; }
}

[DynamoMapper(Convention = DynamoNamingConvention.CamelCase)]
public static partial class ProductMapper
{
    public static partial Dictionary<string, AttributeValue> FromModel(Product source);
    public static partial Product ToModel(Dictionary<string, AttributeValue> item);

    static partial void AfterFromModel(Product source, Dictionary<string, AttributeValue> item)
    {
        // Merge additional attributes
        if (source.AdditionalAttributes != null)
        {
            foreach (var kvp in source.AdditionalAttributes)
            {
                item[kvp.Key] = kvp.Value;
            }
        }
    }

    static partial void AfterToModel(Dictionary<string, AttributeValue> item, ref Product entity)
    {
        // Capture unmapped attributes
        var mappedKeys = new HashSet<string>
        {
            "pk", "sk", "productId", "name", "price"
        };

        entity.AdditionalAttributes = item
            .Where(kvp => !mappedKeys.Contains(kvp.Key))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }
}
```

### Post-Hydration Normalization

Normalize data and populate computed properties after mapping:

```csharp
public class User
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }

    // Computed property
    public string FullName { get; set; }
}

[DynamoMapper(Convention = DynamoNamingConvention.CamelCase)]
public static partial class UserMapper
{
    [DynamoIgnore(nameof(User.FullName))]
    public static partial Dictionary<string, AttributeValue> FromModel(User source);

    [DynamoIgnore(nameof(User.FullName))]
    public static partial User ToModel(Dictionary<string, AttributeValue> item);

    static partial void AfterToModel(Dictionary<string, AttributeValue> item, ref User entity)
    {
        // Normalize email to lowercase
        entity.Email = entity.Email?.ToLowerInvariant();

        // Populate computed property
        entity.FullName = $"{entity.FirstName} {entity.LastName}";
    }
}
```

### GSI Keys

Add Global Secondary Index keys:

```csharp
public class Order
{
    public Guid OrderId { get; set; }
    public string CustomerId { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

[DynamoMapper(Convention = DynamoNamingConvention.CamelCase)]
public static partial class OrderMapper
{
    public static partial Dictionary<string, AttributeValue> FromModel(Order source);
    public static partial Order ToModel(Dictionary<string, AttributeValue> item);

    static partial void AfterFromModel(Order source, Dictionary<string, AttributeValue> item)
    {
        // Main table keys
        item["pk"] = new AttributeValue { S = $"CUSTOMER#{source.CustomerId}" };
        item["sk"] = new AttributeValue { S = $"ORDER#{source.OrderId}" };

        // GSI for querying by status
        item["gsi1pk"] = new AttributeValue { S = $"STATUS#{source.Status.Name}" };
        item["gsi1sk"] = new AttributeValue { S = source.CreatedAt.ToString("O") };
    }
}
```

## Advanced Patterns

### Complete Single-Table Example

A comprehensive single-table design with multiple entity types:

```csharp
public class Order
{
    public Guid OrderId { get; set; }
    public string CustomerId { get; set; }
    public OrderStatus Status { get; set; }
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
    public Dictionary<string, AttributeValue>? Metadata { get; set; }
}

[DynamoMapper(Convention = DynamoNamingConvention.CamelCase)]
public static partial class OrderMapper
{
    public static partial Dictionary<string, AttributeValue> FromModel(Order source);
    public static partial Order ToModel(Dictionary<string, AttributeValue> item);

    static partial void AfterFromModel(Order source, Dictionary<string, AttributeValue> item)
    {
        // Primary access pattern: Get order by customer
        item["pk"] = new AttributeValue { S = $"CUSTOMER#{source.CustomerId}" };
        item["sk"] = new AttributeValue { S = $"ORDER#{source.OrderId}#{source.CreatedAt:yyyy-MM-dd}" };

        // Entity type for polymorphic queries
        item["entityType"] = new AttributeValue { S = "Order" };
        item["recordType"] = new AttributeValue { S = "Order" };

        // GSI1: Query orders by status
        item["gsi1pk"] = new AttributeValue { S = $"STATUS#{source.Status.Name}" };
        item["gsi1sk"] = new AttributeValue { S = source.CreatedAt.ToString("O") };

        // GSI2: Query orders by date range
        item["gsi2pk"] = new AttributeValue { S = "ORDER" };
        item["gsi2sk"] = new AttributeValue { S = source.CreatedAt.ToString("O") };

        // Merge metadata bag
        if (source.Metadata != null)
        {
            foreach (var kvp in source.Metadata)
            {
                item[kvp.Key] = kvp.Value;
            }
        }
    }

    static partial void BeforeToModel(Dictionary<string, AttributeValue> item)
    {
        // Validate entity type before mapping
        if (item.TryGetValue("entityType", out var typeAttr) && typeAttr.S != "Order")
        {
            throw new DynamoMappingException(
                targetType: "Order",
                details: $"Cannot deserialize {typeAttr.S} as Order");
        }
    }

    static partial void AfterToModel(Dictionary<string, AttributeValue> item, ref Order entity)
    {
        // Capture metadata
        var coreKeys = new HashSet<string>
        {
            "pk", "sk", "gsi1pk", "gsi1sk", "gsi2pk", "gsi2sk",
            "entityType", "recordType",
            "orderId", "customerId", "status", "total", "createdAt"
        };

        entity.Metadata = item
            .Where(kvp => !coreKeys.Contains(kvp.Key))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }
}
```

### Conditional Logic in Hooks

```csharp
[DynamoMapper(Convention = DynamoNamingConvention.CamelCase)]
public static partial class ProductMapper
{
    public static partial Dictionary<string, AttributeValue> FromModel(Product source);
    public static partial Product ToModel(Dictionary<string, AttributeValue> item);

    static partial void AfterFromModel(Product source, Dictionary<string, AttributeValue> item)
    {
        // Conditional keys based on product type
        if (source.IsDigital)
        {
            item["pk"] = new AttributeValue { S = $"DIGITAL#{source.Category}" };
            item["sk"] = new AttributeValue { S = $"PRODUCT#{source.ProductId}" };
        }
        else
        {
            item["pk"] = new AttributeValue { S = $"PHYSICAL#{source.Warehouse}" };
            item["sk"] = new AttributeValue { S = $"PRODUCT#{source.ProductId}" };
        }

        // Add inventory tracking only for physical products
        if (!source.IsDigital)
        {
            item["inventoryRequired"] = new AttributeValue { BOOL = true };
        }
    }
}
```

## Hook Execution Order

Hooks execute in a deterministic, predictable order:

### During FromModel:
1. Create empty `Dictionary<string, AttributeValue>`
2. **BeforeFromModel(source, item)** - item is empty
3. Map all configured properties to item
4. **AfterFromModel(source, item)** - item has all mapped properties
5. Return item

### During ToModel:
1. Receive `Dictionary<string, AttributeValue>` from DynamoDB
2. **BeforeToModel(item)** - item is unmodified
3. Map properties and construct entity using object initializer
4. **AfterToModel(item, ref entity)** - entity is constructed and populated
5. Return entity

## Performance Characteristics

### Zero-Cost Abstraction

Unimplemented hooks compile away completely:

```csharp
// If BeforeFromModel is not implemented:
public static partial Dictionary<string, AttributeValue> FromModel(Product source)
{
    var item = new Dictionary<string, AttributeValue>(5);

    // BeforeFromModel call is removed by compiler (partial void)

    // Property mapping...
    item["productId"] = new AttributeValue { S = source.ProductId.ToString() };

    AfterFromModel(source, item); // Only this hook is implemented

    return item;
}
```

### No Reflection

Hooks are statically bound at compile time. No runtime discovery or reflection overhead.

### Minimal Allocation

Generated code reuses the same item dictionary instance across hook calls.

## DSL Integration (Phase 2)

In Phase 2, hooks can be configured via DSL (though partial methods remain the recommended approach):

```csharp
[DynamoMapper]
public static partial class OrderMapper
{
    public static partial Dictionary<string, AttributeValue> FromModel(Order source);
    public static partial Order ToModel(Dictionary<string, AttributeValue> item);

    static partial void Configure(DynamoMapBuilder<Order> map)
    {
        map.BeforeFromModel((source, item) =>
        {
            // Limited DSL hook support
            item["pk"] = new AttributeValue { S = $"CUSTOMER#{source.CustomerId}" };
        });
    }

    // Partial method hooks are still supported and recommended for complex logic
    static partial void AfterFromModel(Order source, Dictionary<string, AttributeValue> item)
    {
        item["sk"] = new AttributeValue { S = $"ORDER#{source.OrderId}" };
        item["recordType"] = new AttributeValue { S = "Order" };
    }
}
```

Note: DSL hooks have limited expression support. Partial method hooks are more powerful and flexible.

## Best Practices

1. **Keep hooks focused and single-purpose**
   ```csharp
   // Good: focused on keys
   static partial void AfterFromModel(Product source, Dictionary<string, AttributeValue> item)
   {
       item["pk"] = new AttributeValue { S = $"PRODUCT#{source.ProductId}" };
       item["sk"] = new AttributeValue { S = "METADATA" };
   }

   // Avoid: mixing concerns
   static partial void AfterFromModel(Product source, Dictionary<string, AttributeValue> item)
   {
       item["pk"] = new AttributeValue { S = $"PRODUCT#{source.ProductId}" };
       // Don't do business logic in hooks
       SendProductAnalyticsEvent(source);
       UpdateInventoryCache(source);
   }
   ```

2. **Use hooks for DynamoDB-specific concerns**
   - Keys (PK, SK, GSI keys)
   - Record types
   - TTLs
   - Attribute bags
   - Not for business logic

3. **Consider separate files for complex hooks**
   - Keeps mapper configuration clean
   - Easier to maintain
   - Better organization

4. **Document hook behavior**
   ```csharp
   /// <summary>
   /// Adds single-table design keys for Customer entities.
   /// PK: CUSTOMER#{CustomerId}
   /// SK: METADATA
   /// </summary>
   static partial void AfterFromModel(Customer source, Dictionary<string, AttributeValue> item)
   {
       item["pk"] = new AttributeValue { S = $"CUSTOMER#{source.CustomerId}" };
       item["sk"] = new AttributeValue { S = "METADATA" };
   }
   ```

5. **Validate entity types early**
   ```csharp
   static partial void BeforeToModel(Dictionary<string, AttributeValue> item)
   {
       if (!item.TryGetValue("entityType", out var typeAttr) || typeAttr.S != "Product")
       {
           throw new DynamoMappingException("Invalid entity type for ProductMapper");
       }
   }
   ```

6. **Use AfterToModel for computed properties**
   - Don't add computed properties to DynamoDB
   - Hydrate them after mapping

## Diagnostics

The generator validates hook signatures and emits diagnostics for common errors:

- **DM0401**: Hook signature doesn't match expected format
- **DM0402**: Hook method is not static
- **DM0403**: Hook parameter types don't match entity type

## See Also

- [Single-Table Design Examples](../examples/single-table-design.md) - Comprehensive single-table patterns
- [Phase 1 Requirements](../roadmap/phase-1.md#11-customization-hooks-phase-1) - Detailed hook specifications
- [Phase 2 DSL](../roadmap/phase-2.md#9-single-table-dynamodb-support-dsl) - DSL hook configuration