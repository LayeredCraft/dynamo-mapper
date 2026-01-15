---
title: How It Works
description: Understanding DynamoMapper's source generation and mapping architecture
---

# How It Works

DynamoMapper is an incremental source generator that produces high-performance, type-safe mapping code at compile time. This guide explains the core concepts, architecture, and design decisions that make DynamoMapper both powerful and focused.

## Core Philosophy

DynamoMapper is built on three fundamental principles:

1. **Domain Stays Clean** - Your domain models remain free of persistence attributes
2. **Compile-Time Safety** - All mapping code is generated and validated at compile time
3. **DynamoDB-Focused** - Single-purpose library for DynamoDB attribute mapping

## Mapping Scope

**DynamoMapper is a DynamoDB-specific mapping library**, not a general-purpose object mapper.

### What DynamoMapper Does

DynamoMapper supports **exactly two mapping directions**:

```csharp
// FromModel: Domain model → DynamoDB item
Dictionary<string, AttributeValue> FromModel(T source);

// ToModel: DynamoDB item → Domain model
T ToModel(Dictionary<string, AttributeValue> item);
```

### What DynamoMapper Does NOT Do

Unlike general-purpose mappers (Mapperly, AutoMapper, etc.), DynamoMapper does **not** support:

- Object-to-object mapping (e.g., `DTO → Entity`)
- Collection transformations
- Projection mapping
- Runtime mapping configuration
- Multi-step mapping pipelines

**Why this matters:** This focused scope enables DynamoMapper to optimize specifically for DynamoDB patterns like single-table design, PK/SK composition, and attribute bags without compromising on performance or clarity.

## Source Generation Overview

DynamoMapper uses .NET's `IIncrementalGenerator` API to analyze your code at compile time and generate mapping implementations.

### The Generation Pipeline

1. **Discovery Phase**
   - Locate classes marked with `[DynamoMapper]`
   - Find partial mapping methods (`FromModel`, `ToModel`)
   - Collect configuration attributes

2. **Analysis Phase**
   - Resolve target entity types
   - Analyze properties (public, readable, writable)
   - Apply naming conventions
   - Validate converters and hooks

3. **Code Generation Phase**
  - Generate `FromModel` implementation
  - Generate `ToModel` implementation
   - Emit diagnostics for configuration errors

4. **Compilation Phase**
   - Generated code is compiled with your project
   - No runtime dependencies beyond AWS SDK types

## Mapper Anatomy

### Basic Structure

```csharp
using Amazon.DynamoDBv2.Model;

[DynamoMapper(Convention = DynamoNamingConvention.CamelCase)]
public static partial class ProductMapper
{
    // Partial method declarations (you provide)
    public static partial Dictionary<string, AttributeValue> FromModel(Product source);
    public static partial Product ToModel(Dictionary<string, AttributeValue> item);

    // Generated implementations (DynamoMapper provides)
    // - FromModel implementation
    // - ToModel implementation
}
```

### Why Static Partial Classes?

- **Static** - No instance state, no object allocation overhead
- **Partial** - You declare, generator implements
- **Type-safe** - Compiler validates everything

## Generated Code Example

Given this domain model:

```csharp
public class Product
{
    public Guid ProductId { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}

[DynamoMapper(Convention = DynamoNamingConvention.CamelCase)]
public static partial class ProductMapper
{
    public static partial Dictionary<string, AttributeValue> FromModel(Product source);
    public static partial Product ToModel(Dictionary<string, AttributeValue> item);
}
```

DynamoMapper generates:

```csharp
public static partial class ProductMapper
{
    public static partial Dictionary<string, AttributeValue> FromModel(Product source)
    {
        var item = new Dictionary<string, AttributeValue>(capacity: 3);

        item["productId"] = new AttributeValue { S = source.ProductId.ToString() };
        item["name"] = new AttributeValue { S = source.Name };
        item["price"] = new AttributeValue { N = source.Price.ToString(CultureInfo.InvariantCulture) };

        return item;
    }

    public static partial Product ToModel(Dictionary<string, AttributeValue> item)
    {
        var entity = new Product
        {
            ProductId = Guid.Parse(item["productId"].S),
            Name = item["name"].S,
            Price = decimal.Parse(item["price"].N, CultureInfo.InvariantCulture)
        };

        return entity;
    }
}
```

### Key Characteristics

- **Direct property access** - No reflection
- **Culture-invariant parsing** - Consistent number handling
- **Capacity hints** - Reduces dictionary reallocations
- **Clear, readable code** - Easy to debug

## Configuration Model

DynamoMapper uses a layered configuration model:

### 1. Mapper-Level Defaults

```csharp
[DynamoMapper(
    Convention = DynamoNamingConvention.CamelCase,
    OmitNullStrings = true,
    DateTimeFormat = "O")]
```

### 2. Property-Level Overrides

```csharp
[DynamoField(nameof(Product.Name), Required = true)]
[DynamoField(nameof(Product.Description), OmitIfNullOrWhiteSpace = true)]
public static partial Dictionary<string, AttributeValue> FromModel(Product source);
```

### 3. Converters

Two approaches, both first-class:

```csharp
// Converter type
[DynamoField(nameof(Product.Status), Converter = typeof(StatusConverter))]

// Static methods
[DynamoField(nameof(Product.Status),
    ToMethod = nameof(ToStatus),
    FromMethod = nameof(FromStatus))]
```

### 4. Customization Hooks

```csharp
static partial void AfterFromModel(Product source, Dictionary<string, AttributeValue> item)
{
    item["pk"] = new AttributeValue { S = $"PRODUCT#{source.ProductId}" };
    item["sk"] = new AttributeValue { S = "METADATA" };
}
```

## Type System

### Supported Types (Phase 1)

DynamoMapper natively supports:

| .NET Type | DynamoDB Type | Notes |
|-----------|---------------|-------|
| `string` | S (String) | |
| `int`, `long`, `decimal`, `double` | N (Number) | Culture-invariant |
| `bool` | BOOL | |
| `Guid` | S | ToString/Parse |
| `DateTime`, `DateTimeOffset` | S | ISO-8601 |
| `enum` | S | String name |
| Nullable variants | S/N/BOOL | Null checks generated |

### Custom Types

Use converters for custom types:

```csharp
public class OrderStatus
{
    public string Name { get; }
    // ... enumeration pattern
}

public class OrderStatusConverter : IDynamoConverter<OrderStatus>
{
    public AttributeValue ToAttributeValue(OrderStatus value)
        => new AttributeValue { S = value.Name };

    public OrderStatus FromAttributeValue(AttributeValue value)
        => OrderStatus.FromName(value.S);
}
```

## Naming Conventions

DynamoMapper applies naming conventions to property names:

```csharp
// .NET Property    → DynamoDB Attribute Name

// Exact
ProductId         → ProductId

// CamelCase (recommended)
ProductId         → productId
CustomerId        → customerId

// SnakeCase
ProductId         → product_id
CustomerId        → customer_id
```

Override per-property:

```csharp
[DynamoField(nameof(Product.ProductId), Name = "id")]
```

## Extensibility Model

### Hooks: First-Class Extension Points

Hooks enable DynamoDB-specific patterns without compromising the focused mapping scope:

```csharp
// Before property mapping
static partial void BeforeFromModel(Product source, Dictionary<string, AttributeValue> item);

// After property mapping - most common
static partial void AfterFromModel(Product source, Dictionary<string, AttributeValue> item);

// Before deserialization
static partial void BeforeToModel(Dictionary<string, AttributeValue> item);

// After object construction
static partial void AfterToModel(Dictionary<string, AttributeValue> item, ref Product entity);
```

**Common use cases:**
- PK/SK composition for single-table design
- Record type discrimination
- TTL attributes
- Unmapped attribute bags
- Post-hydration normalization

### Why Hooks Instead of Pipeline Stages?

DynamoMapper uses hooks instead of a general mapping pipeline because:

1. **Focused scope** - Only two mapping directions, not arbitrary transformations
2. **Zero overhead** - Unimplemented hooks compile away completely
3. **Type safety** - Statically bound, no reflection
4. **DynamoDB patterns** - Designed specifically for single-table design

## Diagnostics

DynamoMapper provides comprehensive compile-time diagnostics:

### Error Classes

- **DM0xxx** - Configuration errors (missing types, invalid attributes)
- **DM02xx** - Converter errors (signature mismatch, type incompatibility)
- **DM03xx** - DSL errors (invalid expressions, precedence conflicts)
- **DM04xx** - Hook errors (signature validation)

### Example Diagnostic

```
error DM0201: Static conversion method 'ToStatus' not found on mapper 'OrderMapper'
  Location: OrderMapper.cs(12,5)
  Fix: Add static method: static AttributeValue ToStatus(OrderStatus value)
```

## Performance Characteristics

### Compile-Time

- Incremental generation - Only affected mappers regenerate
- Deterministic output - Same input always produces same code
- Fast compilation - No expensive analysis

### Runtime

- **Zero reflection** - All types resolved at compile time
- **Minimal allocations** - Dictionary capacity hints, no LINQ
- **Inlined conversions** - Simple type conversions inlined by JIT
- **Culture-invariant parsing** - Consistent, fast number handling

### Benchmarks (Typical)

| Operation                | Time    | Allocations    |
|--------------------------|---------|----------------|
| FromModel (5 properties) | ~50ns   | 1 (dictionary) |
| ToModel (5 properties)   | ~100ns  | 1 (entity)     |
| With hooks               | +5-10ns | 0 additional   |

## Design Constraints

Understanding what DynamoMapper intentionally does NOT support:

### No Runtime Configuration

```csharp
// Not supported - all configuration is compile-time
mapper.Configure(x => x.Property("Name").Ignore());
```

**Why:** Compile-time configuration enables:
- Zero reflection
- Faster runtime performance
- Compile-time validation

### No Dynamic Mapping

```csharp
// Not supported - all types known at compile time
var mapper = MapperFactory.Create(type1, type2);
```

**Why:** Type-safe, analyzable, debuggable generated code.

### No Nested Object Mapping (Phase 1)

```csharp
// Not supported in Phase 1
public class Order
{
    public Customer Customer { get; set; }  // Nested object
}
```

**Why:** Phase 1 focuses on scalar properties. Use converters for complex types.

## Phase 2: DSL Configuration

Phase 2 adds an optional fluent DSL while maintaining all Phase 1 capabilities:

```csharp
[DynamoMapper]
public static partial class ProductMapper
{
    public static partial Dictionary<string, AttributeValue> FromModel(Product source);
    public static partial Product ToModel(Dictionary<string, AttributeValue> item);

    static partial void Configure(DynamoMapBuilder<Product> map)
    {
        map.Naming(DynamoNamingConvention.CamelCase);

        map.Property(x => x.Status)
           .Using<StatusConverter>();

        map.Ignore(x => x.ComputedProperty);
    }
}
```

**Key points:**
- DSL is **optional** - attributes remain fully supported
- DSL is **compile-time only** - no runtime evaluation
- DSL and attributes can coexist - DSL takes precedence

## Best Practices

1. **Use default conventions** - Override only when necessary
2. **Keep domain clean** - No DynamoDB concerns in domain models
3. **Use hooks for DynamoDB patterns** - PK/SK, TTL, record types
4. **Prefer converter types for reusable logic** - Testable, composable
5. **Use static methods for one-off conversions** - Simple, co-located

## See Also

- [Customization Hooks](../usage/customization-hooks.md) - Extending mapping behavior
- [Converters](../usage/converters.md) - Custom type conversion
- [Phase 1 Requirements](../roadmap/phase-1.md) - Complete Phase 1 specification
- [Phase 2 Requirements](../roadmap/phase-2.md) - DSL configuration