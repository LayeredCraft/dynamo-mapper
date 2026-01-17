---
title: Customization Hooks
description: Using partial methods to extend DynamoMapper mapping behavior
---

# Customization Hooks

Customization hooks are first-class extension points in DynamoMapper that allow you to inject custom logic into the mapping pipeline. Implemented as optional partial methods, hooks enable powerful patterns like single-table design, record discrimination, and attribute passthrough without sacrificing performance or type safety.

## Overview

DynamoMapper provides **four lifecycle hooks** that execute at strategic points during mapping:

| Hook | Phase | When | Use For |
|------|-------|------|---------|
| `BeforeToItem` | ToItem | Before property mapping | Pre-processing, initialization |
| `AfterToItem` | ToItem | After property mapping | PK/SK injection, TTL, metadata |
| `BeforeFromItem` | FromItem | Before property mapping | Validation, preprocessing |
| `AfterFromItem` | FromItem | After object construction | Normalization, computed properties |

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

### BeforeToItem

Invoked before property mapping during `ToItem`.

```csharp
static partial void BeforeToItem(T source, Dictionary<string, AttributeValue> item);
```

**Parameters:**
- `source` - The domain object being mapped
- `item` - An empty dictionary that will be populated

**Use cases:**
- Initialize dictionary capacity
- Add fixed metadata before mapping
- Pre-compute derived values

### AfterToItem

Invoked after property mapping during `ToItem`.

```csharp
static partial void AfterToItem(T source, Dictionary<string, AttributeValue> item);
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

### BeforeFromItem

Invoked before property mapping during `FromItem`.

```csharp
static partial void BeforeFromItem(Dictionary<string, AttributeValue> item);
```

**Parameters:**
- `item` - The raw DynamoDB item dictionary

**Use cases:**
- Validate required metadata
- Transform item structure
- Extract unmapped attributes
- Log or audit incoming data

### AfterFromItem

Invoked after property mapping and object construction during `FromItem`.

```csharp
static partial void AfterFromItem(Dictionary<string, AttributeValue> item, ref T entity);
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
using DynamoMapper.Runtime;

[DynamoMapper(Convention = DynamoNamingConvention.CamelCase)]
public static partial class ProductMapper
{
    public static partial Dictionary<string, AttributeValue> ToItem(Product source);
    public static partial Product FromItem(Dictionary<string, AttributeValue> item);

    static partial void AfterToItem(Product source, Dictionary<string, AttributeValue> item)
    {
        item.SetString("pk", $"PRODUCT#{source.ProductId}");
        item.SetString("sk", "METADATA");
        item.SetString("recordType", "Product");
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
    public static partial Dictionary<string, AttributeValue> ToItem(Product source);
    public static partial Product FromItem(Dictionary<string, AttributeValue> item);
}

// ProductMapper.Hooks.cs
public static partial class ProductMapper
{
    static partial void BeforeToItem(Product source, Dictionary<string, AttributeValue> item)
    {
        // Pre-processing logic
    }

    static partial void AfterToItem(Product source, Dictionary<string, AttributeValue> item)
    {
        item.SetString("pk", $"PRODUCT#{source.ProductId}");
        item.SetString("sk", "METADATA");
        item.SetString("recordType", "Product");
    }

    static partial void AfterFromItem(Dictionary<string, AttributeValue> item, ref Product entity)
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
    public static partial Dictionary<string, AttributeValue> ToItem(Order source);
    public static partial Order FromItem(Dictionary<string, AttributeValue> item);

    static partial void AfterToItem(Order source, Dictionary<string, AttributeValue> item)
    {
        // Composite keys for single-table design
        item.SetString("pk", $"CUSTOMER#{source.CustomerId}");
        item.SetString("sk", $"ORDER#{source.OrderId}");
        item.SetString("recordType", "Order");
    }
}
```

### Record Type Discrimination

Validate entity types on read:

```csharp
[DynamoMapper(Convention = DynamoNamingConvention.CamelCase)]
public static partial class CustomerMapper
{
    public static partial Dictionary<string, AttributeValue> ToItem(Customer source);
    public static partial Customer FromItem(Dictionary<string, AttributeValue> item);

    static partial void AfterToItem(Customer source, Dictionary<string, AttributeValue> item)
    {
        item.SetString("entityType", nameof(Customer));
    }

    static partial void AfterFromItem(Dictionary<string, AttributeValue> item, ref Customer entity)
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
    public static partial Dictionary<string, AttributeValue> ToItem(Session source);
    public static partial Session FromItem(Dictionary<string, AttributeValue> item);

    static partial void AfterToItem(Session source, Dictionary<string, AttributeValue> item)
    {
        // Set TTL to 24 hours from now
        var ttl = DateTimeOffset.UtcNow.AddHours(24).ToUnixTimeSeconds();
        item.SetLong("ttl", ttl);
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
    public static partial Dictionary<string, AttributeValue> ToItem(Product source);
    public static partial Product FromItem(Dictionary<string, AttributeValue> item);

    static partial void AfterToItem(Product source, Dictionary<string, AttributeValue> item)
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

    static partial void AfterFromItem(Dictionary<string, AttributeValue> item, ref Product entity)
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
[DynamoIgnore(nameof(User.FullName))]
public static partial class UserMapper
{
    public static partial Dictionary<string, AttributeValue> ToItem(User source);

    public static partial User FromItem(Dictionary<string, AttributeValue> item);

    static partial void AfterFromItem(Dictionary<string, AttributeValue> item, ref User entity)
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
    public static partial Dictionary<string, AttributeValue> ToItem(Order source);
    public static partial Order FromItem(Dictionary<string, AttributeValue> item);

    static partial void AfterToItem(Order source, Dictionary<string, AttributeValue> item)
    {
        // Main table keys
        item.SetString("pk", $"CUSTOMER#{source.CustomerId}");
        item.SetString("sk", $"ORDER#{source.OrderId}");

        // GSI for querying by status
        item.SetString("gsi1pk", $"STATUS#{source.Status.Name}");
        item.SetString("gsi1sk", source.CreatedAt.ToString("O"));
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
    public static partial Dictionary<string, AttributeValue> ToItem(Order source);
    public static partial Order FromItem(Dictionary<string, AttributeValue> item);

    static partial void AfterToItem(Order source, Dictionary<string, AttributeValue> item)
    {
        // Primary access pattern: Get order by customer
        item.SetString("pk", $"CUSTOMER#{source.CustomerId}");
        item.SetString("sk", $"ORDER#{source.OrderId}#{source.CreatedAt:yyyy-MM-dd}");

        // Entity type for polymorphic queries
        item.SetString("entityType", "Order");
        item.SetString("recordType", "Order");

        // GSI1: Query orders by status
        item.SetString("gsi1pk", $"STATUS#{source.Status.Name}");
        item.SetString("gsi1sk", source.CreatedAt.ToString("O"));

        // GSI2: Query orders by date range
        item.SetString("gsi2pk", "ORDER");
        item.SetString("gsi2sk", source.CreatedAt.ToString("O"));

        // Merge metadata bag
        if (source.Metadata != null)
        {
            foreach (var kvp in source.Metadata)
            {
                item[kvp.Key] = kvp.Value;
            }
        }
    }

    static partial void BeforeFromItem(Dictionary<string, AttributeValue> item)
    {
        // Validate entity type before mapping
        if (item.TryGetValue("entityType", out var typeAttr) && typeAttr.S != "Order")
        {
            throw new DynamoMappingException(
                targetType: "Order",
                details: $"Cannot deserialize {typeAttr.S} as Order");
        }
    }

    static partial void AfterFromItem(Dictionary<string, AttributeValue> item, ref Order entity)
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
    public static partial Dictionary<string, AttributeValue> ToItem(Product source);
    public static partial Product FromItem(Dictionary<string, AttributeValue> item);

    static partial void AfterToItem(Product source, Dictionary<string, AttributeValue> item)
    {
        // Conditional keys based on product type
        if (source.IsDigital)
        {
            item.SetString("pk", $"DIGITAL#{source.Category}");
            item.SetString("sk", $"PRODUCT#{source.ProductId}");
        }
        else
        {
            item.SetString("pk", $"PHYSICAL#{source.Warehouse}");
            item.SetString("sk", $"PRODUCT#{source.ProductId}");
        }

        // Add inventory tracking only for physical products
        if (!source.IsDigital)
        {
            item.SetBool("inventoryRequired", true);
        }
    }
}
```

## Hook Execution Order

Hooks execute in a deterministic, predictable order:

### During ToItem:
1. Create empty `Dictionary<string, AttributeValue>`
2. **BeforeToItem(source, item)** - item is empty
3. Map all configured properties to item
4. **AfterToItem(source, item)** - item has all mapped properties
5. Return item

### During FromItem:
1. Receive `Dictionary<string, AttributeValue>` from DynamoDB
2. **BeforeFromItem(item)** - item is unmodified
3. Map properties and construct entity using object initializer
4. **AfterFromItem(item, ref entity)** - entity is constructed and populated
5. Return entity

## Performance Characteristics

### Zero-Cost Abstraction

Unimplemented hooks compile away completely:

```csharp
// If no hooks are implemented:
public static partial Dictionary<string, AttributeValue> ToItem(Product source) =>
    new Dictionary<string, AttributeValue>(1)
        .SetGuid("productId", source.ProductId, false, true);
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
    public static partial Dictionary<string, AttributeValue> ToItem(Order source);
    public static partial Order FromItem(Dictionary<string, AttributeValue> item);

    static partial void Configure(DynamoMapBuilder<Order> map)
    {
        map.BeforeToItem((source, item) =>
        {
            // Limited DSL hook support
            item.SetString("pk", $"CUSTOMER#{source.CustomerId}");
        });
    }

    // Partial method hooks are still supported and recommended for complex logic
    static partial void AfterToItem(Order source, Dictionary<string, AttributeValue> item)
    {
        item.SetString("sk", $"ORDER#{source.OrderId}");
        item.SetString("recordType", "Order");
    }
}
```

Note: DSL hooks have limited expression support. Partial method hooks are more powerful and flexible.

## Best Practices

1. **Keep hooks focused and single-purpose**
   ```csharp
   // Good: focused on keys
   static partial void AfterToItem(Product source, Dictionary<string, AttributeValue> item)
   {
       item.SetString("pk", $"PRODUCT#{source.ProductId}");
       item.SetString("sk", "METADATA");
   }

   // Avoid: mixing concerns
   static partial void AfterToItem(Product source, Dictionary<string, AttributeValue> item)
   {
       item.SetString("pk", $"PRODUCT#{source.ProductId}");
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
   static partial void AfterToItem(Customer source, Dictionary<string, AttributeValue> item)
   {
       item.SetString("pk", $"CUSTOMER#{source.CustomerId}");
       item.SetString("sk", "METADATA");
   }
   ```

5. **Validate entity types early**
   ```csharp
   static partial void BeforeFromItem(Dictionary<string, AttributeValue> item)
   {
       if (!item.TryGetValue("entityType", out var typeAttr) || typeAttr.S != "Product")
       {
           throw new DynamoMappingException("Invalid entity type for ProductMapper");
       }
   }
   ```

6. **Use AfterFromItem for computed properties**
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
