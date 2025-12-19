---
title: Phase 2 Requirements
description: Fluent DSL configuration for DynamoMapper - comprehensive Phase 2 specification
---

# DynamoMapper – Phase 2 Requirements (Fluent DSL Configuration) 🚀

> **Scope:** Phase 2 introduces an **optional fluent DSL** for configuring DynamoMapper mappings, providing a strongly-typed, refactor-safe alternative to attribute-based configuration introduced in Phase 1.
>
> **Key principle:** Phase 2 **does not replace** Phase 1. Attribute-based configuration remains fully supported and first-class. The DSL is additive and opt-in.
>
> **Important:** Like Phase 1, DynamoMapper remains a **DynamoDB-specific mapping library**, not a general-purpose object mapper. The DSL supports only two mapping directions: `T → Dictionary<string, AttributeValue>` and `Dictionary<string, AttributeValue> → T`. The DSL does not introduce general-purpose mapping capabilities beyond DynamoDB.

---

## 1. Phase 2 Goals

### 1.1 Why Phase 2 Exists
While attribute-based configuration is performant and familiar, it has limitations:
- Verbose for entities with many properties
- Relies on string-based member references (`nameof(...)`)
- Harder to express cross-cutting rules (defaults, filters, capture-unmapped)
- Less readable for complex single-table DynamoDB patterns

The Phase 2 DSL aims to:
- Improve **developer ergonomics**
- Preserve **compile-time safety**
- Maintain **generator performance and determinism**
- Express advanced mapping scenarios cleanly

---

## 2. Design Constraints (Non-Negotiable)

The DSL **must** adhere to the following constraints:

1. **Incremental generator friendly**
   - Only a *single, well-known configuration method* per mapper
   - Generator inspects only that method’s syntax tree

2. **Restricted grammar**
   - No arbitrary C# execution
   - No dynamic behavior
   - No runtime evaluation

3. **Deterministic**
   - Same source input → same generated output

4. **No reflection at runtime**
   - DSL exists purely at compile time

5. **Attributes remain supported**
   - DSL and attributes can coexist
   - Clear precedence rules defined

---

## 3. User-Facing API

### 3.1 DSL Entry Point

Each mapper may optionally define a **single configuration method**:

```csharp
public static partial class JediCharacterMapper
{
    static partial void Configure(DynamoMapBuilder<JediCharacter> map);
}
```

Rules:
- Method name **must** be `Configure`
- Must be `static partial void`
- Must accept exactly one parameter of type `DynamoMapBuilder<T>`
- Generic `T` must match mapper target type

---

## 4. DynamoMapBuilder<T>

### 4.1 Responsibilities
`DynamoMapBuilder<T>` represents a compile-time mapping configuration surface.

It allows:
- Property inclusion / exclusion
- Field naming
- Requiredness
- Omission rules
- Converter selection
- Global defaults
- Single-table helpers

This type **must not** contain runtime logic; it is a marker API for the generator.

---

## 5. Property Configuration DSL

### 5.1 Property Selection
Properties are selected using simple member access expressions:

```csharp
map.Property(x => x.OwnerId)
```

Constraints:
- Lambda must be `x => x.Property`
- No method calls
- No computed expressions
- No nested access (`x => x.Sub.Prop`) in Phase 2.0

Violations produce diagnostics.

---

### 5.2 Property Configuration Chain

```csharp
map.Property(x => x.OwnerId)
   .Name("ownerId")
   .Required()
   .AsString();
```

#### Supported Fluent Methods (Phase 2.0)

| Method | Description |
|---|---|
| `Name(string)` | Overrides DynamoDB attribute name |
| `Required()` | Marks property as required |
| `Optional()` | Marks property as optional |
| `AsString()` | Forces `AttributeValue.S` |
| `AsNumber()` | Forces `AttributeValue.N` |
| `AsBool()` | Forces `AttributeValue.BOOL` |
| `OmitIfNull()` | Omits when null |
| `OmitIfNullOrWhiteSpace()` | Omits when null/empty/whitespace |
| `Using<TConverter>()` | Uses custom converter |

---

## 6. Ignoring Properties

```csharp
map.Ignore(x => x.CriticalHitChance);
```

Rules:
- Ignored properties must exist
- Cannot both configure and ignore the same property
- Ignored properties are excluded from both directions

---

## 7. Global Configuration (Mapper-Level DSL)

### 7.1 Naming Convention

```csharp
map.Naming(DynamoNamingConvention.CamelCase);
```

### 7.2 Requiredness Defaults

```csharp
map.DefaultRequiredness(DefaultRequiredness.InferFromNullability);
```

### 7.3 Omission Defaults

```csharp
map.OmitNullStrings();
map.OmitEmptyStrings();
map.OmitNullValues();
```

---

## 8. Converters (DSL)

The Phase 2 DSL supports **both converter approaches** introduced in Phase 1, ensuring parity and consistency between attribute-based and DSL-based configuration.

### 8.1 Approach 1: Converter Types (Recommended)

Use the `.Using<TConverter>()` fluent method to apply a converter type:

```csharp
map.Property(x => x.Status)
   .Using<OrderStatusConverter>();
```

**Requirements:**
- Converter must implement `IDynamoConverter<T>`
- Generic type `T` must match property type
- Generator validates type compatibility at compile time

**Example:**

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

public class OrderStatusConverter : IDynamoConverter<OrderStatus>
{
    public AttributeValue ToAttributeValue(OrderStatus value)
        => new AttributeValue { S = value.Name };

    public OrderStatus FromAttributeValue(AttributeValue value)
        => OrderStatus.FromName(value.S);
}
```

### 8.2 Approach 2: Named Static Methods

Use the `.Using(toMethod, fromMethod)` fluent method to reference static conversion methods:

```csharp
map.Property(x => x.Status)
   .Using(nameof(ToOrderStatus), nameof(FromOrderStatus));
```

**Requirements:**
- Both method names must be provided (no partial specification)
- Methods must be `static` on the mapper class
- Method signatures must match: `AttributeValue ToX(T)` and `T FromX(AttributeValue)`
- Methods must be accessible from generated code

**Example:**

```csharp
[DynamoMapper]
public static partial class OrderMapper
{
    public static partial Dictionary<string, AttributeValue> ToItem(Order source);
    public static partial Order FromItem(Dictionary<string, AttributeValue> item);

    static partial void Configure(DynamoMapBuilder<Order> map)
    {
        map.Property(x => x.Status)
           .Using(nameof(ToOrderStatus), nameof(FromOrderStatus));
    }

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

### 8.3 DSL Constraints and Validation

The generator validates converter configuration and emits diagnostics for:

**Converter Type Errors:**
- **DM0301**: Converter type does not implement `IDynamoConverter<T>`
- **DM0302**: Converter generic type mismatch with property type

**Static Method Errors:**
- **DM0303**: Static conversion method not found
- **DM0304**: Static conversion method has invalid signature
- **DM0305**: Both ToMethod and FromMethod must be specified together
- **DM0306**: Cannot specify both converter type and static methods

### 8.4 Choosing Between Approaches in DSL

The same guidance from Phase 1 applies:

**Use converter types** (`.Using<TConverter>()`) when:
- Conversion logic is reusable across multiple mappers
- Complex conversion benefits from testability
- You prefer explicit, dedicated converter classes

**Use static methods** (`.Using(toMethod, fromMethod)`) when:
- Conversion is specific to one mapper
- Logic is simple and inline
- You prefer co-location with configuration

Both approaches generate identical runtime code and have equivalent performance.

### 8.5 Mixing DSL and Attribute Converter Configuration

When both DSL and attributes specify converters for the same property:

1. **DSL converter takes precedence** over attribute converter
2. Generator emits a **warning diagnostic** (DM0307) about the override
3. Attribute converter is ignored

Example (DSL wins):

```csharp
[DynamoMapper]
public static partial class ProductMapper
{
    // Attribute specifies one converter
    [DynamoField(nameof(Product.Category), Converter = typeof(OldCategoryConverter))]
    public static partial Dictionary<string, AttributeValue> ToItem(Product source);

    public static partial Product FromItem(Dictionary<string, AttributeValue> item);

    static partial void Configure(DynamoMapBuilder<Product> map)
    {
        // DSL overrides attribute - NewCategoryConverter is used
        map.Property(x => x.Category)
           .Using<NewCategoryConverter>();
    }
}
```

### 8.6 Cross-References

See also:
- [Phase 1 Converters](phase-1.md#9-converters-phase-1)
- [Converter Types Documentation](../../docs/usage/converters.md)
- [Static Converter Documentation](../../docs/usage/static-converters.md)

---

## 9. Single-Table DynamoDB Support (DSL)

### 9.1 Lifecycle Hooks

Hooks defined via DSL are equivalent to Phase 1 partial methods.

```csharp
map.BeforeToItem((source, item) =>
{
    item["pk"] = new AttributeValue { S = source.Pk };
    item["sk"] = new AttributeValue { S = source.Sk };
});
```

Constraints:
- Lambda bodies are **not executed**
- Generator only parses known assignment patterns
- Complex logic is disallowed in Phase 2.0

---

### 9.2 Capture Unmapped Attributes

```csharp
map.CaptureUnmappedAttributes(
    target: x => x.Keys,
    filter: key => key != "pk" && key != "sk"
);
```

This enables:
- Forward compatibility
- Sparse attributes
- Single-table extensibility

---

## 10. Precedence Rules (Attributes + DSL)

When both attributes and DSL are present:

1. **DSL wins over attributes**
2. Attributes fill gaps where DSL is silent
3. Duplicate/conflicting definitions produce diagnostics

---

## 11. Generated Code Expectations

Generated output **must be identical** to Phase 1 output for equivalent configurations.

DSL configuration is compiled into the same internal mapping model as attributes.

---

## 12. Diagnostics (Phase 2)

### 12.1 Required Diagnostics
- Invalid property selector expressions
- Unsupported fluent call chains
- Duplicate property configuration
- Converter type mismatches
- Conflicts between DSL and attributes

### 12.2 Diagnostic Philosophy
- Fail fast
- Clear messages
- Point to exact DSL invocation

---

## 13. Backward Compatibility

- Existing attribute-based mappers continue to work unchanged
- DSL is optional and opt-in
- Phase 1 attributes remain supported indefinitely

---

## 14. Testing Requirements

### 14.1 DSL Parsing Tests
- Valid fluent chains
- Invalid expressions
- Conflict resolution

### 14.2 Golden Output Tests
- Attribute vs DSL equivalence
- Mixed configuration scenarios

---

## 15. Documentation (Phase 2)

GitHub Pages must include:
- DSL overview and rationale
- Side-by-side examples (Attributes vs DSL)
- Supported fluent API reference
- Limitations and constraints
- Migration guide from attributes to DSL

---

## 16. Explicit Non-Goals (Phase 2)

Phase 2 does **not** include:
- Arbitrary C# execution in DSL
- Runtime evaluation of lambdas
- Nested object mapping
- UpdateExpression DSL (future phase)
- Query or scan builders

---

## Appendix A – Example Full DSL Mapper

```csharp
[DynamoMapper]
public static partial class JediCharacterMapper
{
    static partial void Configure(DynamoMapBuilder<JediCharacter> map)
    {
        map.Naming(DynamoNamingConvention.CamelCase);

        map.Property(x => x.OwnerId).Required();
        map.Property(x => x.CharacterId).AsString().Required();

        map.Property(x => x.Alignment)
           .Name("forceAlignment")
           .Using<EnumerationAsStringConverter<ForceAlignment>>();

        map.Ignore(x => x.CriticalHitChance);
        map.Ignore(x => x.HitRoll);
        map.Ignore(x => x.DamRoll);

        map.BeforeToItem((src, item) =>
        {
            item["pk"] = new AttributeValue { S = src.Pk };
            item["sk"] = new AttributeValue { S = src.Sk };
        });
    }
}
```

---

**End of Phase 2 Requirements** 🎯
