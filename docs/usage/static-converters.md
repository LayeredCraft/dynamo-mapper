---
title: Static Conversion Methods
description: Using named static methods for inline DynamoDB attribute conversion
---

# Static Conversion Methods

Static conversion methods provide an inline, lightweight approach to custom type conversion in DynamoMapper. Unlike converter types, static methods are defined directly on the mapper class, making them ideal for simple, mapper-specific conversions.

## Overview

DynamoMapper supports two approaches for custom conversions:

1. **Converter types** implementing `IDynamoConverter<T>` (recommended for reusable conversions)
2. **Named static methods** on the mapper class (convenient for one-off conversions)

This guide focuses on **static conversion methods**, which are inspired by Mapperly's conversion method pattern.

## When to Use Static Methods

Use static conversion methods when:

- Conversion logic is specific to a single mapper
- The conversion is simple and doesn't warrant a separate type
- You prefer co-location of mapping configuration and conversion logic
- No reuse across multiple mappers is needed

For reusable, testable conversions, consider using [converter types](converters.md) instead.

## Required Method Signatures

Static conversion methods must follow exact signatures:

```csharp
// Conversion TO DynamoDB AttributeValue
static AttributeValue ToMethodName(TProperty value);

// Conversion FROM DynamoDB AttributeValue
static TProperty FromMethodName(AttributeValue value);
```

**Requirements:**
- Methods must be `static`
- Methods must be declared on the mapper class
- Return type and parameter type must match exactly as shown
- Both To and From methods must be provided together

## Basic Example

```csharp
using Amazon.DynamoDBv2.Model;

public class OrderStatus
{
    public string Name { get; }
    public int Value { get; }

    private OrderStatus(string name, int value)
    {
        Name = name;
        Value = value;
    }

    public static readonly OrderStatus Pending = new("Pending", 0);
    public static readonly OrderStatus Confirmed = new("Confirmed", 1);
    public static readonly OrderStatus Shipped = new("Shipped", 2);

    public static OrderStatus FromName(string name) => name switch
    {
        "Pending" => Pending,
        "Confirmed" => Confirmed,
        "Shipped" => Shipped,
        _ => throw new ArgumentException($"Unknown status: {name}")
    };
}

public class Order
{
    public Guid OrderId { get; set; }
    public OrderStatus Status { get; set; }
    public decimal Total { get; set; }
}

[DynamoMapper(Convention = DynamoNamingConvention.CamelCase)]
public static partial class OrderMapper
{
    [DynamoField(nameof(Order.Status), ToMethod = nameof(ToOrderStatus), FromMethod = nameof(FromOrderStatus))]
    public static partial Dictionary<string, AttributeValue> FromModel(Order source);

    public static partial Order ToModel(Dictionary<string, AttributeValue> item);

    // Static conversion methods
    static AttributeValue ToOrderStatus(OrderStatus status)
    {
        return new AttributeValue { S = status.Name };
    }

    static OrderStatus FromOrderStatus(AttributeValue value)
    {
        return OrderStatus.FromName(value.S);
    }
}
```

## Attribute Usage (Phase 1)

### DynamoField Attribute

Configure static methods using the `[DynamoField]` attribute:

```csharp
[DynamoField(nameof(Order.Status),
    ToMethod = nameof(ToOrderStatus),
    FromMethod = nameof(FromOrderStatus))]
public static partial Dictionary<string, AttributeValue> FromModel(Order source);
```

**Properties:**
- `ToMethod` - Name of the static method for To conversion (required)
- `FromMethod` - Name of the static method for From conversion (required)

**Constraints:**
- Both `ToMethod` and `FromMethod` must be specified together
- Cannot specify `Converter` and `ToMethod`/`FromMethod` simultaneously
- Method names must reference existing, accessible static methods

## DSL Usage (Phase 2)

In Phase 2, static methods can be configured using the fluent DSL:

```csharp
[DynamoMapper]
public static partial class OrderMapper
{
    public static partial Dictionary<string, AttributeValue> FromModel(Order source);
    public static partial Order ToModel(Dictionary<string, AttributeValue> item);

    static partial void Configure(DynamoMapBuilder<Order> map)
    {
        map.Property(x => x.Status)
           .Using(nameof(ToOrderStatus), nameof(FromOrderStatus));
    }

    static AttributeValue ToOrderStatus(OrderStatus status)
    {
        return new AttributeValue { S = status.Name };
    }

    static OrderStatus FromOrderStatus(AttributeValue value)
    {
        return OrderStatus.FromName(value.S);
    }
}
```

## Multiple Static Conversions

You can define multiple static conversion method pairs in a single mapper:

```csharp
[DynamoMapper(Convention = DynamoNamingConvention.CamelCase)]
public static partial class ProductMapper
{
    [DynamoField(nameof(Product.Category), ToMethod = nameof(ToCategory), FromMethod = nameof(FromCategory))]
    [DynamoField(nameof(Product.Status), ToMethod = nameof(ToProductStatus), FromMethod = nameof(FromProductStatus))]
    public static partial Dictionary<string, AttributeValue> FromModel(Product source);

    public static partial Product ToModel(Dictionary<string, AttributeValue> item);

    // Category conversion
    static AttributeValue ToCategory(ProductCategory category)
    {
        return new AttributeValue { S = category.Code };
    }

    static ProductCategory FromCategory(AttributeValue value)
    {
        return ProductCategory.FromCode(value.S);
    }

    // Status conversion
    static AttributeValue ToProductStatus(ProductStatus status)
    {
        return new AttributeValue { S = status.Name };
    }

    static ProductStatus FromProductStatus(AttributeValue value)
    {
        return ProductStatus.FromName(value.S);
    }
}
```

## Nullable Types

The generator automatically handles nullable types. Your static methods should work with the non-nullable type:

```csharp
public class User
{
    public Guid UserId { get; set; }
    public UserRole? Role { get; set; }  // Nullable
}

[DynamoMapper(Convention = DynamoNamingConvention.CamelCase)]
public static partial class UserMapper
{
    [DynamoField(nameof(User.Role), ToMethod = nameof(ToUserRole), FromMethod = nameof(FromUserRole))]
    public static partial Dictionary<string, AttributeValue> FromModel(User source);

    public static partial User ToModel(Dictionary<string, AttributeValue> item);

    // Methods work with non-nullable UserRole
    static AttributeValue ToUserRole(UserRole role)
    {
        return new AttributeValue { S = role.Name };
    }

    static UserRole FromUserRole(AttributeValue value)
    {
        return UserRole.FromName(value.S);
    }
}
```

The generator wraps your methods with null checks as needed.

## Advanced Example: Complex Value Objects

Static methods work well for complex value objects:

```csharp
public class Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public override string ToString() => $"{Amount:F2} {Currency}";

    public static Money Parse(string value)
    {
        var parts = value.Split(' ');
        return new Money(decimal.Parse(parts[0]), parts[1]);
    }
}

[DynamoMapper(Convention = DynamoNamingConvention.CamelCase)]
public static partial class ProductMapper
{
    [DynamoField(nameof(Product.Price), ToMethod = nameof(ToMoney), FromMethod = nameof(FromMoney))]
    public static partial Dictionary<string, AttributeValue> FromModel(Product source);

    public static partial Product ToModel(Dictionary<string, AttributeValue> item);

    static AttributeValue ToMoney(Money money)
    {
        // Store as string: "99.99 USD"
        return new AttributeValue { S = money.ToString() };
    }

    static Money FromMoney(AttributeValue value)
    {
        return Money.Parse(value.S);
    }
}
```

## Diagnostics for Invalid Signatures

DynamoMapper validates static method signatures and emits compile-time diagnostics:

### DM0201: Static Conversion Method Not Found

Emitted when a referenced method doesn't exist:

```csharp
[DynamoField(nameof(Order.Status),
    ToMethod = nameof(ToStatus),  // Method doesn't exist
    FromMethod = nameof(FromStatus))]
```

**Fix:** Ensure the method exists and is accessible.

### DM0202: Static Conversion Method Has Invalid Signature

Emitted when method signature doesn't match requirements:

```csharp
// Wrong: returns void instead of AttributeValue
static void ToOrderStatus(OrderStatus status) { }

// Wrong: parameter is string instead of OrderStatus
static AttributeValue ToOrderStatus(string status) { }

// Correct:
static AttributeValue ToOrderStatus(OrderStatus status) { }
```

**Fix:** Ensure methods match exact signatures: `AttributeValue ToX(T)` and `T FromX(AttributeValue)`.

### DM0203: Static Conversion Method Type Mismatch

Emitted when method parameter/return types don't align with property type:

```csharp
// Property is OrderStatus, but method uses ProductStatus
static AttributeValue ToOrderStatus(ProductStatus status) { }
```

**Fix:** Ensure parameter and return types match the property type.

### DM0204: Both ToMethod and FromMethod Required

Emitted when only one method is specified:

```csharp
// Wrong: only ToMethod specified
[DynamoField(nameof(Order.Status), ToMethod = nameof(ToOrderStatus))]
```

**Fix:** Provide both `ToMethod` and `FromMethod`.

### DM0205: Cannot Specify Both Converter and Static Methods

Emitted when both approaches are used simultaneously:

```csharp
// Wrong: cannot use both
[DynamoField(nameof(Order.Status),
    Converter = typeof(OrderStatusConverter),
    ToMethod = nameof(ToOrderStatus),
    FromMethod = nameof(FromOrderStatus))]
```

**Fix:** Choose one approach (converter type or static methods).

## Comparison with Converter Types

| Feature | Static Methods | Converter Types |
|---------|---------------|-----------------|
| Reusability | Single mapper only | Across multiple mappers |
| Testability | Must test via mapper | Test in isolation |
| Organization | Co-located with config | Separate files/projects |
| Verbosity | Less boilerplate | More explicit |
| DI Support | No | Future phases |
| Best For | Simple, mapper-specific | Complex, reusable |

## Best Practices

1. **Use clear naming conventions**
   ```csharp
   // Good: Clear what type is being converted
   static AttributeValue ToOrderStatus(OrderStatus status) { }
   static OrderStatus FromOrderStatus(AttributeValue value) { }

   // Avoid: Ambiguous method names
   static AttributeValue Convert(OrderStatus status) { }
   ```

2. **Keep methods simple**
   - Static methods should focus on conversion logic only
   - Complex business logic belongs elsewhere

3. **Handle errors gracefully**
   ```csharp
   static OrderStatus FromOrderStatus(AttributeValue value)
   {
       if (string.IsNullOrEmpty(value.S))
           throw new DynamoMappingException("OrderStatus cannot be null or empty");

       return OrderStatus.FromName(value.S);
   }
   ```

4. **Use static methods for mapper-specific conversions**
   - If conversion logic is reused across mappers, use converter types instead

5. **Document complex conversions**
   ```csharp
   /// <summary>
   /// Converts Money to DynamoDB string format: "amount currency" (e.g., "99.99 USD")
   /// </summary>
   static AttributeValue ToMoney(Money money) { }
   ```

## See Also

- [Converter Types](converters.md) - Reusable converter types implementing `IDynamoConverter<T>`
- [Phase 1 Requirements](../roadmap/phase-1.md#9-converters-phase-1) - Detailed converter specifications
- [Phase 2 DSL](../roadmap/phase-2.md#8-converters-dsl) - DSL converter configuration