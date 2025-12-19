---
title: Converter Types
description: Using IDynamoConverter<T> for reusable DynamoDB attribute conversion
---

# Converter Types

Converter types provide a reusable, testable approach to custom type conversion in DynamoMapper. By implementing the `IDynamoConverter<T>` interface, you can create dedicated converter classes that can be shared across multiple mappers and tested in isolation.

## Overview

DynamoMapper supports two approaches for custom conversions:

1. **Converter types** implementing `IDynamoConverter<T>` (recommended for reusable conversions)
2. **Named static methods** on the mapper class (convenient for one-off conversions)

This guide focuses on **converter types**, which are the recommended approach for most scenarios.

## When to Use Converter Types

Use converter types when:

- Conversion logic is reusable across multiple mappers
- Complex conversion logic benefits from testability
- You want clear separation between mapping configuration and conversion logic
- You may need dependency injection in future phases

For simple, mapper-specific conversions, consider using [static methods](static-converters.md) instead.

## The IDynamoConverter<T> Interface

The `IDynamoConverter<T>` interface defines the contract for type conversion:

```csharp
namespace LayeredCraft.DynamoMapper.Runtime;

/// <summary>
/// Defines conversion between a .NET type and DynamoDB AttributeValue.
/// </summary>
/// <typeparam name="T">The .NET type to convert</typeparam>
public interface IDynamoConverter<T>
{
    /// <summary>
    /// Converts a .NET value to a DynamoDB AttributeValue.
    /// </summary>
    /// <param name="value">The value to convert</param>
    /// <returns>The DynamoDB AttributeValue representation</returns>
    AttributeValue ToAttributeValue(T value);

    /// <summary>
    /// Converts a DynamoDB AttributeValue to a .NET value.
    /// </summary>
    /// <param name="value">The AttributeValue to convert</param>
    /// <returns>The .NET value</returns>
    T FromAttributeValue(AttributeValue value);
}
```

## Basic Example

Here's a simple converter for an enumeration-style class:

```csharp
using Amazon.DynamoDBv2.Model;
using LayeredCraft.DynamoMapper.Runtime;

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
    public static readonly OrderStatus Delivered = new("Delivered", 3);

    public static OrderStatus FromName(string name) => name switch
    {
        "Pending" => Pending,
        "Confirmed" => Confirmed,
        "Shipped" => Shipped,
        "Delivered" => Delivered,
        _ => throw new ArgumentException($"Unknown status: {name}")
    };
}

public class OrderStatusConverter : IDynamoConverter<OrderStatus>
{
    public AttributeValue ToAttributeValue(OrderStatus value)
    {
        return new AttributeValue { S = value.Name };
    }

    public OrderStatus FromAttributeValue(AttributeValue value)
    {
        if (string.IsNullOrEmpty(value.S))
            throw new DynamoMappingException("OrderStatus AttributeValue cannot be null or empty");

        return OrderStatus.FromName(value.S);
    }
}
```

## Using Converters in Mappers

### Attribute Usage (Phase 1)

Configure converters using the `[DynamoField]` attribute:

```csharp
using Amazon.DynamoDBv2.Model;

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
    [DynamoField(nameof(Order.Status), Converter = typeof(OrderStatusConverter))]
    public static partial Dictionary<string, AttributeValue> ToItem(Order source);

    public static partial Order FromItem(Dictionary<string, AttributeValue> item);
}
```

### DSL Usage (Phase 2)

In Phase 2, converters can be configured using the fluent DSL:

```csharp
[DynamoMapper]
public static partial class OrderMapper
{
    public static partial Dictionary<string, AttributeValue> ToItem(Order source);
    public static partial Order FromItem(Dictionary<string, AttributeValue> item);

    static partial void Configure(DynamoMapBuilder<Order> map)
    {
        map.Property(x => x.Status)
           .Using<OrderStatusConverter>();
    }
}
```

## Handling Nullable Types

The generator automatically wraps converter calls with null checks for nullable types. Your converter should work with the non-nullable type:

```csharp
public class User
{
    public Guid UserId { get; set; }
    public string Email { get; set; }
    public UserRole? Role { get; set; }  // Nullable
}

// Converter works with non-nullable UserRole
public class UserRoleConverter : IDynamoConverter<UserRole>
{
    public AttributeValue ToAttributeValue(UserRole value)
    {
        return new AttributeValue { S = value.Name };
    }

    public UserRole FromAttributeValue(AttributeValue value)
    {
        return UserRole.FromName(value.S);
    }
}

[DynamoMapper(Convention = DynamoNamingConvention.CamelCase)]
public static partial class UserMapper
{
    [DynamoField(nameof(User.Role), Converter = typeof(UserRoleConverter))]
    public static partial Dictionary<string, AttributeValue> ToItem(User source);

    public static partial Order FromItem(Dictionary<string, AttributeValue> item);
}
```

The generator produces code similar to:

```csharp
// Generated ToItem code
if (source.Role != null)
{
    var converter = new UserRoleConverter();
    item["role"] = converter.ToAttributeValue(source.Role);
}

// Generated FromItem code
UserRole? role = null;
if (item.TryGetValue("role", out var roleAttr))
{
    var converter = new UserRoleConverter();
    role = converter.FromAttributeValue(roleAttr);
}
```

## Advanced Examples

### Complex Value Objects

Converters work well for complex value objects:

```csharp
public class Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative");
        if (string.IsNullOrEmpty(currency))
            throw new ArgumentException("Currency is required");

        Amount = amount;
        Currency = currency;
    }

    public override string ToString() => $"{Amount:F2} {Currency}";

    public static Money Parse(string value)
    {
        var parts = value.Split(' ');
        if (parts.Length != 2)
            throw new FormatException("Invalid money format");

        return new Money(decimal.Parse(parts[0]), parts[1]);
    }
}

public class MoneyConverter : IDynamoConverter<Money>
{
    public AttributeValue ToAttributeValue(Money value)
    {
        // Store as string: "99.99 USD"
        return new AttributeValue { S = value.ToString() };
    }

    public Money FromAttributeValue(AttributeValue value)
    {
        if (string.IsNullOrEmpty(value.S))
            throw new DynamoMappingException("Money AttributeValue cannot be null or empty");

        try
        {
            return Money.Parse(value.S);
        }
        catch (FormatException ex)
        {
            throw new DynamoMappingException($"Invalid money format: {value.S}", ex);
        }
    }
}
```

### Storing as Number

You can store values as DynamoDB numbers:

```csharp
public class Percentage
{
    public decimal Value { get; }

    public Percentage(decimal value)
    {
        if (value < 0 || value > 100)
            throw new ArgumentException("Percentage must be between 0 and 100");

        Value = value;
    }
}

public class PercentageConverter : IDynamoConverter<Percentage>
{
    public AttributeValue ToAttributeValue(Percentage value)
    {
        // Store as decimal number
        return new AttributeValue
        {
            N = value.Value.ToString(CultureInfo.InvariantCulture)
        };
    }

    public Percentage FromAttributeValue(AttributeValue value)
    {
        if (string.IsNullOrEmpty(value.N))
            throw new DynamoMappingException("Percentage AttributeValue.N cannot be null");

        var decimalValue = decimal.Parse(value.N, CultureInfo.InvariantCulture);
        return new Percentage(decimalValue);
    }
}
```

### JSON Serialization for Complex Objects

For complex nested structures (when Phase 1 doesn't support them natively), you can use JSON:

```csharp
using System.Text.Json;

public class Address
{
    public string Street { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string ZipCode { get; set; }
}

public class AddressConverter : IDynamoConverter<Address>
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public AttributeValue ToAttributeValue(Address value)
    {
        var json = JsonSerializer.Serialize(value, Options);
        return new AttributeValue { S = json };
    }

    public Address FromAttributeValue(AttributeValue value)
    {
        if (string.IsNullOrEmpty(value.S))
            throw new DynamoMappingException("Address AttributeValue cannot be null");

        try
        {
            return JsonSerializer.Deserialize<Address>(value.S, Options)
                ?? throw new DynamoMappingException("Deserialized address is null");
        }
        catch (JsonException ex)
        {
            throw new DynamoMappingException($"Failed to deserialize address: {value.S}", ex);
        }
    }
}
```

### Generic Converters

Create generic converters for common patterns:

```csharp
/// <summary>
/// Generic converter for any type that has ToString/Parse methods
/// </summary>
public class StringSerializableConverter<T> : IDynamoConverter<T>
    where T : IParsable<T>
{
    public AttributeValue ToAttributeValue(T value)
    {
        return new AttributeValue { S = value.ToString() };
    }

    public T FromAttributeValue(AttributeValue value)
    {
        if (string.IsNullOrEmpty(value.S))
            throw new DynamoMappingException($"{typeof(T).Name} AttributeValue cannot be null");

        return T.Parse(value.S, null);
    }
}

// Usage with DateOnly (implements IParsable<DateOnly>)
[DynamoField(nameof(Product.ReleaseDate), Converter = typeof(StringSerializableConverter<DateOnly>))]
```

## Reusing Converters Across Mappers

One of the key benefits of converter types is reusability:

```csharp
// Define converter once
public class OrderStatusConverter : IDynamoConverter<OrderStatus>
{
    public AttributeValue ToAttributeValue(OrderStatus value)
        => new AttributeValue { S = value.Name };

    public OrderStatus FromAttributeValue(AttributeValue value)
        => OrderStatus.FromName(value.S);
}

// Use in multiple mappers
[DynamoMapper(Convention = DynamoNamingConvention.CamelCase)]
public static partial class OrderMapper
{
    [DynamoField(nameof(Order.Status), Converter = typeof(OrderStatusConverter))]
    public static partial Dictionary<string, AttributeValue> ToItem(Order source);
    public static partial Order FromItem(Dictionary<string, AttributeValue> item);
}

[DynamoMapper(Convention = DynamoNamingConvention.CamelCase)]
public static partial class OrderHistoryMapper
{
    [DynamoField(nameof(OrderHistory.PreviousStatus), Converter = typeof(OrderStatusConverter))]
    [DynamoField(nameof(OrderHistory.CurrentStatus), Converter = typeof(OrderStatusConverter))]
    public static partial Dictionary<string, AttributeValue> ToItem(OrderHistory source);
    public static partial OrderHistory FromItem(Dictionary<string, AttributeValue> item);
}
```

## Testing Converters

Converters are easy to test in isolation:

```csharp
using Xunit;
using Amazon.DynamoDBv2.Model;

public class OrderStatusConverterTests
{
    private readonly OrderStatusConverter _converter = new();

    [Fact]
    public void ToAttributeValue_ValidStatus_ReturnsStringAttribute()
    {
        // Arrange
        var status = OrderStatus.Pending;

        // Act
        var result = _converter.ToAttributeValue(status);

        // Assert
        Assert.NotNull(result.S);
        Assert.Equal("Pending", result.S);
    }

    [Fact]
    public void FromAttributeValue_ValidString_ReturnsOrderStatus()
    {
        // Arrange
        var attr = new AttributeValue { S = "Confirmed" };

        // Act
        var result = _converter.FromAttributeValue(attr);

        // Assert
        Assert.Equal(OrderStatus.Confirmed, result);
    }

    [Fact]
    public void FromAttributeValue_InvalidString_ThrowsException()
    {
        // Arrange
        var attr = new AttributeValue { S = "InvalidStatus" };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _converter.FromAttributeValue(attr));
    }

    [Fact]
    public void FromAttributeValue_NullString_ThrowsException()
    {
        // Arrange
        var attr = new AttributeValue { S = null };

        // Act & Assert
        Assert.Throws<DynamoMappingException>(() => _converter.FromAttributeValue(attr));
    }
}
```

## Error Handling Best Practices

Always validate input and provide clear error messages:

```csharp
public class CustomerTypeConverter : IDynamoConverter<CustomerType>
{
    public AttributeValue ToAttributeValue(CustomerType value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value), "CustomerType cannot be null");

        return new AttributeValue { S = value.Code };
    }

    public CustomerType FromAttributeValue(AttributeValue value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value), "AttributeValue cannot be null");

        if (string.IsNullOrWhiteSpace(value.S))
        {
            throw new DynamoMappingException(
                fieldName: "customerType",
                details: "AttributeValue.S is null or empty");
        }

        try
        {
            return CustomerType.FromCode(value.S);
        }
        catch (ArgumentException ex)
        {
            throw new DynamoMappingException(
                fieldName: "customerType",
                details: $"Invalid customer type code: '{value.S}'",
                innerException: ex);
        }
    }
}
```

## Performance Considerations

### Converter Instantiation

The generator instantiates converters using `new()` for each conversion. Keep constructors lightweight:

```csharp
// Good: lightweight constructor
public class OrderStatusConverter : IDynamoConverter<OrderStatus>
{
    public AttributeValue ToAttributeValue(OrderStatus value)
        => new AttributeValue { S = value.Name };

    public OrderStatus FromAttributeValue(AttributeValue value)
        => OrderStatus.FromName(value.S);
}

// Avoid: heavy initialization in constructor
public class BadConverter : IDynamoConverter<SomeType>
{
    private readonly ExpensiveService _service;

    public BadConverter()
    {
        _service = new ExpensiveService(); // Don't do this
    }
}
```

### Caching Static Data

If you need expensive initialization, use static fields:

```csharp
public class CategoryConverter : IDynamoConverter<Category>
{
    // Static dictionary initialized once
    private static readonly Dictionary<string, Category> Cache =
        Category.GetAll().ToDictionary(c => c.Code);

    public AttributeValue ToAttributeValue(Category value)
        => new AttributeValue { S = value.Code };

    public Category FromAttributeValue(AttributeValue value)
        => Cache.TryGetValue(value.S, out var category)
            ? category
            : throw new DynamoMappingException($"Unknown category: {value.S}");
}
```

## Comparison with Static Methods

| Feature | Converter Types | Static Methods |
|---------|----------------|----------------|
| Reusability | Across multiple mappers | Single mapper only |
| Testability | Easy to test in isolation | Test via mapper |
| Organization | Separate files/projects | Co-located with mapper |
| Verbosity | More explicit | Less boilerplate |
| DI Support | Future phases | No |
| Best For | Complex, reusable | Simple, mapper-specific |

## Best Practices

1. **One converter per type**
   ```csharp
   // Good: focused converter
   public class OrderStatusConverter : IDynamoConverter<OrderStatus> { }

   // Avoid: generic "do everything" converter
   public class MegaConverter : IDynamoConverter<object> { }
   ```

2. **Validate inputs thoroughly**
   - Check for null AttributeValues
   - Validate AttributeValue kinds (S, N, BOOL, etc.)
   - Provide meaningful error messages

3. **Use culture-invariant formatting for numbers**
   ```csharp
   public AttributeValue ToAttributeValue(decimal value)
   {
       return new AttributeValue
       {
           N = value.ToString(CultureInfo.InvariantCulture)
       };
   }
   ```

4. **Document complex conversion logic**
   ```csharp
   /// <summary>
   /// Converts Money to DynamoDB string format.
   /// Format: "amount currency" (e.g., "99.99 USD")
   /// </summary>
   public class MoneyConverter : IDynamoConverter<Money> { }
   ```

5. **Keep converters stateless**
   - Converters should not maintain mutable state
   - Use static fields for immutable cached data only

6. **Throw DynamoMappingException for mapping errors**
   ```csharp
   catch (Exception ex)
   {
       throw new DynamoMappingException(
           fieldName: "price",
           details: "Failed to parse money value",
           innerException: ex);
   }
   ```

## Built-in Converters

DynamoMapper includes several built-in converters:

- `GuidAsStringConverter` - Converts `Guid` to/from string (S)
- `DateTimeIsoStringConverter` - Converts `DateTime` to/from ISO-8601 string
- `EnumAsStringConverter<TEnum>` - Converts enums to/from string names

Example usage:

```csharp
[DynamoField(nameof(Product.ProductId), Converter = typeof(GuidAsStringConverter))]
```

## See Also

- [Static Conversion Methods](static-converters.md) - Alternative inline conversion approach
- [Phase 1 Requirements](../roadmap/phase-1.md#9-converters-phase-1) - Detailed converter specifications
- [Phase 2 DSL](../roadmap/phase-2.md#8-converters-dsl) - DSL converter configuration