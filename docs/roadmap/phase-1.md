---
title: Phase 1 Requirements
description: Attribute-based mapping for DynamoMapper - comprehensive Phase 1 specification
---

# DynamoMapper – Phase 1 Requirements (Attribute-Based Mapping) ✅

> **Scope:** Phase 1 delivers a Mapperly-style, attribute-configured **incremental source generator** that generates high-performance mapping code between **domain models** and **Amazon DynamoDB `AttributeValue` dictionaries**, without requiring any attributes on the domain models themselves.
>
> **Core philosophy:** *Domain stays clean.* All configuration lives on mapper methods (and optionally the mapper type). Generated code is explicit, allocation-conscious, and friendly to single-table DynamoDB patterns.
>
> **Important:** DynamoMapper is a **DynamoDB-specific mapping library**, not a general-purpose object mapper. It supports only two mapping directions: `T → Dictionary<string, AttributeValue>` and `Dictionary<string, AttributeValue> → T`. Unlike general-purpose mappers like Mapperly, DynamoMapper focuses exclusively on DynamoDB attribute mapping and single-table patterns.

---

## 1. Background and Goals

### 1.1 Problem Statement

Manually writing `FromModel()` and `ToModel()` methods for DynamoDB is repetitive and error-prone:
- Field naming drift (`OwnerId` vs `ownerId`)
- `S` vs `N` (string vs number) mistakes
- Inconsistent null handling (omit vs store empty)
- Boilerplate parsing (`GetInt`, `GetGuid`, etc.)
- Hard-to-maintain mapping logic for single-table items

### 1.2 Phase 1 Objectives
Phase 1 must provide:

1. **Source generation** of mapping code:
   - `T -> Dictionary<string, AttributeValue>`
   - `Dictionary<string, AttributeValue> -> T`

2. **Attribute-based configuration** on mapper surfaces (Mapperly-style):
   - No attributes required on domain models.
   - Map configuration via method attributes (repeatable).

3. **Convention-first with selective overrides**
   - Default mapping for most properties.
   - Minimal attribute usage for edge cases.

4. **High-performance runtime behavior**
   - No reflection at runtime.
   - Minimal allocations; use `Dictionary<string, AttributeValue>` with indexer assignment.
   - Deterministic conversions and culture-safe number formatting.

5. **Diagnostics and correctness**
   - Compile-time diagnostics where possible.
   - Clear runtime exceptions for missing required fields.

6. **Packaging**
   - `DynamoMapper` (generator/analyzer) depends on `DynamoMapper.Runtime` (attributes + helpers).

---

## 2. Deliverables

### 2.1 NuGet Packages
#### 2.1.1 `DynamoMapper` (Generator Package)
- Ships the incremental generator as an analyzer.
- Provides no public runtime API beyond referencing runtime package.
- Depends on `DynamoMapper.Runtime`.

#### 2.1.2 `DynamoMapper.Runtime` (Runtime Package)
- Ships public attributes, runtime exception types, and helper APIs.
- Ships `AttributeValue` extension helpers usable independently from generator.

### 2.2 Repository Layout (Recommended)
```
/src
  /LayeredCraft.DynamoMapper.Runtime
  /LayeredCraft.DynamoMapper.Generators
/tests
  /LayeredCraft.DynamoMapper.Generators.Tests
  /LayeredCraft.DynamoMapper.Runtime.Tests (optional)
/docs
  phase-1-requirements.md (this file)
```

---

## 3. User Experience (Developer-Facing)

### 3.1 Minimal Example

```csharp
using Amazon.DynamoDBv2.Model;

namespace MyApp.Persistence;

[DynamoMapper(Convention = DynamoNamingConvention.CamelCase)]
public static partial class JediCharacterMapper
{
    public static partial Dictionary<string, AttributeValue> FromModel(JediCharacter source);

    public static partial JediCharacter ToModel(Dictionary<string, AttributeValue> item);
}
```

With default conventions:
- Maps public settable properties on `JediCharacter`
- Uses camelCase field names
- Uses standard type conversions

### 3.2 Override Example (Mapperly-style method attributes)

```csharp
using Amazon.DynamoDBv2.Model;

namespace MyApp.Persistence;

[DynamoMapper(Convention = DynamoNamingConvention.CamelCase, OmitNullStrings = true)]
public static partial class JediCharacterMapper
{
    [DynamoField(nameof(JediCharacter.Alignment), Name = "forceAlignment")]
    [DynamoField(nameof(JediCharacter.BackStory), Name = "backStory", OmitIfNullOrWhiteSpace = true)]
    [DynamoIgnore(nameof(JediCharacter.CriticalHitChance))]
    [DynamoIgnore(nameof(JediCharacter.HitRoll))]
    [DynamoIgnore(nameof(JediCharacter.DamRoll))]
    public static partial Dictionary<string, AttributeValue> FromModel(JediCharacter source);

    public static partial JediCharacter ToModel(Dictionary<string, AttributeValue> item);
}
```

---

## 4. Supported Mapping Scenarios (Phase 1)

### 4.1 Supported Targets
- **POCO classes**
- **records** (with settable properties)
- `init` setters supported (generator uses object initializer)

**Not required for Phase 1:** mapping to constructors / positional records.

### 4.2 Property Inclusion Rules (Defaults)
By default, generator maps:
- Public instance properties
- That are readable (`get`)
- And writable (`set` or `init`)
- Excluding indexers

Generator ignores by default:
- Getter-only computed properties
- Static properties
- `[DynamoIgnore]`-marked properties (via method-level ignore metadata)

### 4.3 Supported Property Types (Phase 1)
Must support:
- `string`
- `bool` and `bool?`
- `int`, `long`, `double`, `decimal` and nullable variants
- `Guid` and `Guid?`
- `DateTime` and `DateTime?` (stored as ISO-8601 string by default)
- `DateTimeOffset` and nullable (ISO-8601 string by default)
- `TimeSpan` and nullable (ISO-8601 duration string or ticks; choose one and document)
- `enum` (stored as string name by default)
- **Custom converter types** (see §9)

**Not required for Phase 1:**
- Nested complex types (`M`)
- Collections (`L`, `SS`, `NS`, `BS`)
- Polymorphism
- DynamoDB sets

> Note: If you want to include simple lists early, add it to Phase 1.1 later. Phase 1 core assumes scalar mapping.

---

## 5. Naming Conventions

### 5.1 Supported Conventions
`DynamoNamingConvention` must include:
- `Exact` (property name unchanged)
- `CamelCase` (OwnerId → ownerId)
- `SnakeCase` (OwnerId → owner_id) *(optional but easy win)*

### 5.2 Field Name Overrides
Override per property via method attribute:
- `[DynamoField(nameof(Type.Property), Name = "forceAlignment")]`

If override not provided, field name is computed via convention.

---

## 6. DynamoDB AttributeValue Kind Rules

### 6.1 Default Kind Inference
Generator infers `AttributeValue` kind based on property type:

| .NET Type | Default AttributeValue |
|---|---|
| string | `S` |
| numeric types | `N` |
| bool | `BOOL` |
| Guid | `S` (stringified) |
| DateTime / DateTimeOffset | `S` (ISO-8601) |
| enum | `S` (name) |

### 6.2 Kind Override (Escape Hatch)
Allow override per property:
- `[DynamoField(..., Kind = DynamoKind.N)]`
- `[DynamoField(..., Kind = DynamoKind.S)]`

`DynamoKind` (Phase 1) should include at least:
- `S`, `N`, `BOOL`
Optionally include for future compatibility:
- `B`, `M`, `L`, `NULL`, `SS`, `NS`, `BS`

If a kind override is incompatible with the property type, emit **diagnostic**.

---

## 7. Optionality, Required Fields, and Omission

### 7.1 Default Requiredness
Defaults should be:
- Reference types: **required if non-nullable**; optional if nullable
- Value types: required unless nullable

> This behavior must be configurable at mapper level (see §8).

### 7.2 Required Override
Allow explicit requiredness:
- `[DynamoField(nameof(...), Required = true)]`
- `[DynamoField(nameof(...), Required = false)]`

### 7.3 Omit Null/Empty Behavior
Phase 1 must support omission policies:
- Mapper-level:
  - `OmitNullStrings` (omit null string values)
  - `OmitEmptyStrings` (omit empty strings)
  - `OmitNullValues` (for nullable value types)
- Per-field:
  - `OmitIfNullOrWhiteSpace`
  - `OmitIfNull`
  - `OmitIfDefault` *(optional; good for ints if desired)*

If a property is omitted and is **Required**, generated code must:

- **not omit** on FromModel
- and **throw** on ToModel if missing

---

## 8. Mapper Configuration Attributes (Runtime API)

### 8.1 `[DynamoMapper]` Attribute
Applied to a mapper type (static partial class).
Fields:
- `Convention` (default: `CamelCase`)
- `DefaultRequiredness` (default: infer from nullability)
- `OmitNullStrings` (default: true)
- `OmitEmptyStrings` (default: false)
- `DateTimeFormat` (default: `O`/round-trip)
- `EnumFormat` (default: `Name`; optional `Numeric`)

### 8.2 `[DynamoField]` Attribute (Repeatable)
Applied to mapper methods.

Required constructor:
- `string memberName` (recommend users pass `nameof(Type.Member)`)

Named properties:
- `string? Name`
- `bool? Required`
- `DynamoKind? Kind`
- `bool? OmitIfNull`
- `bool? OmitIfNullOrWhiteSpace`
- `Type? Converter` (see §9)
- `string? ConverterName` *(optional, for “named converters” if you add registry later)*

### 8.3 `[DynamoIgnore]` Attribute (Repeatable)
Applied to mapper methods.

Constructor:
- `string memberName`

---

## 9. Converters (Phase 1)

### 9.1 Design Goals
Converters are necessary for:
- Custom "Enumeration" patterns (e.g., `OrderStatus`, `CustomerType`)
- Alternate serialization formats
- Non-standard conversion strategies

Phase 1 supports **two equally first-class conversion approaches**:
1. **Converter types** implementing `IDynamoConverter<T>` (recommended for reusable, testable conversions)
2. **Named static methods** on the mapper class (convenient for one-off, inline conversions)

Both approaches are fully supported and generate identical runtime code.

### 9.2 Approach 1: Converter Types (Recommended)

#### 9.2.1 Converter Interface (Runtime)
Provide a runtime interface usable by generated code:

```csharp
public interface IDynamoConverter<T>
{
    AttributeValue ToAttributeValue(T value);
    T FromAttributeValue(AttributeValue value);
}
```

Support nullable conversion either via:
- separate interface `IDynamoConverter<T?>`, or
- generator wraps null checks.

#### 9.2.2 Example Converter Implementation

```csharp
public class OrderStatusConverter : IDynamoConverter<OrderStatus>
{
    public AttributeValue ToAttributeValue(OrderStatus value)
    {
        return new AttributeValue { S = value.Name };
    }

    public OrderStatus FromAttributeValue(AttributeValue value)
    {
        return OrderStatus.FromName(value.S);
    }
}
```

#### 9.2.3 Using Converter Types
Per field via attribute:
```csharp
[DynamoField(nameof(Order.Status), Converter = typeof(OrderStatusConverter))]
public static partial Dictionary<string, AttributeValue> FromModel(Order source);
```

Constraints:
- Converter must implement `IDynamoConverter<TProperty>`
- If mismatch, emit diagnostic.

#### 9.2.4 Benefits
- Reusable across multiple mappers
- Testable in isolation
- Works with dependency injection (if needed in future phases)
- Clear separation of concerns

### 9.3 Approach 2: Named Static Conversion Methods

#### 9.3.1 Overview
For inline, one-off conversions, users may define static methods on the mapper class with specific signatures.

#### 9.3.2 Required Method Signatures
Static conversion methods must follow these exact signatures:

```csharp
// Conversion TO DynamoDB AttributeValue
static AttributeValue ToMethodName(TProperty value);

// Conversion FROM DynamoDB AttributeValue
static TProperty FromMethodName(AttributeValue value);
```

**Naming convention:** Methods must be paired with symmetric names. By convention, use `To{PropertyName}` and `From{PropertyName}`.

#### 9.3.3 Example Static Method Conversion

```csharp
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

#### 9.3.4 Constraints and Diagnostics
The generator must validate:
- Methods exist and are accessible
- Methods are `static`
- Method signatures match exactly: `AttributeValue ToX(T)` and `T FromX(AttributeValue)`
- Return types and parameter types align with property type

Violations produce compile-time diagnostics:
- **DM0201**: Static conversion method not found
- **DM0202**: Static conversion method has invalid signature
- **DM0203**: Static conversion method parameter/return type mismatch

#### 9.3.5 Benefits
- Concise for simple, one-off conversions
- No additional types required
- Familiar to Mapperly users
- Co-located with mapper configuration

#### 9.3.6 When to Use Static Methods vs Converter Types
Use **static methods** when:
- Conversion is specific to one mapper
- Logic is simple and inline
- No reuse needed across mappers

Use **converter types** when:
- Conversion is reusable across multiple mappers
- Complex conversion logic benefits from testability
- Future DSL integration is desired

### 9.4 Attribute Configuration for Converters

#### 9.4.1 DynamoField Attribute Properties
The `[DynamoField]` attribute supports both approaches:

```csharp
// Converter type approach
[DynamoField(nameof(Order.Status), Converter = typeof(OrderStatusConverter))]

// Static method approach
[DynamoField(nameof(Order.Status), ToMethod = nameof(ToOrderStatus), FromMethod = nameof(FromOrderStatus))]
```

Constraints:
- Cannot specify both `Converter` and `ToMethod`/`FromMethod` simultaneously
- Both `ToMethod` and `FromMethod` must be specified together
- If only one is specified, emit diagnostic

### 9.5 Built-in Converters (Phase 1)
Include minimal built-ins:
- `GuidAsStringConverter`
- `DateTimeIsoStringConverter` (if needed beyond default)
- `EnumAsStringConverter<TEnum>` (optional; generator may inline enums instead)

> Custom patterns (Enumeration, Value Objects, etc.) can be handled by user-provided converters or static methods in Phase 1.

### 9.6 Cross-References
See also:
- [Converter Types Documentation](../../docs/usage/converters.md)
- [Static Converter Documentation](../../docs/usage/static-converters.md)

---

## 10. Generated Code Requirements

### 10.1 General Requirements
Generated code must:
- Be placed in a stable namespace (same namespace as mapper type is recommended)
- Use `partial` method implementations
- Use indexer syntax for dictionaries:
  - `item["ownerId"] = new AttributeValue { S = source.OwnerId };`
- Use `CultureInfo.InvariantCulture` for numeric conversions
- Avoid LINQ in hot paths
- Avoid reflection
- Avoid `ToString()` without culture for numbers

### 10.2 `FromModel` Generation Requirements

Generated `FromModel` must:
- Instantiate a `Dictionary<string, AttributeValue>` with a sensible initial capacity if determinable
- Set mapped fields using configured naming
- Omit fields based on omission policy (unless required)
- Call user hooks (if defined) **at deterministic points** (see §11)
- Return the dictionary

### 10.3 `ToModel` Generation Requirements

Generated `ToModel` must:
- Validate required fields (throw `DynamoMappingException` with detailed message)
- Read each field using `AttributeValue` kinds or converter as configured
- Create target object via object initializer:
  - `var entity = new JediCharacter { OwnerId = ..., ... };`
- Call user hooks (see §11)
- Return the entity

### 10.4 Error Handling and Exceptions
Provide `DynamoMappingException : Exception` in runtime package, with:
- `string? Mapper` (mapper type name)
- `string? TargetType`
- `string? FieldName`
- `string? MemberName`
- `string? Details`

Generated runtime failures should throw `DynamoMappingException`.

---

## 11. Customization Hooks (Phase 1)

Customization hooks are **first-class extension points** that allow developers to inject custom logic into the mapping pipeline. Hooks are implemented as **optional partial methods** on the mapper class.

### 11.1 Hook Signatures

Phase 1 supports four lifecycle hooks that provide access to mapping operations at key points:

#### 11.1.1 BeforeFromModel Hook

Invoked **before** property mapping during `FromModel`.

```csharp
static partial void BeforeFromModel(T source, Dictionary<string, AttributeValue> item);
```

**Parameters:**
- `source`: The source domain object being mapped
- `item`: An empty dictionary that will be populated with mapped properties

**Use cases:**
- Initialize dictionary with fixed capacity hints
- Add metadata before property mapping
- Pre-compute derived values

#### 11.1.2 AfterFromModel Hook

Invoked **after** property mapping during `FromModel`.

```csharp
static partial void AfterFromModel(T source, Dictionary<string, AttributeValue> item);
```

**Parameters:**
- `source`: The source domain object (for reference)
- `item`: The fully populated dictionary with all mapped properties

**Use cases:**
- Add single-table keys (`pk`, `sk`)
- Inject discriminator fields (`recordType`, `entityType`)
- Add TTL attributes
- Merge additional attribute dictionaries
- Override or remove generated attributes

#### 11.1.3 BeforeToModel Hook

Invoked **before** property mapping during `ToModel`.

```csharp
static partial void BeforeToModel(Dictionary<string, AttributeValue> item);
```

**Parameters:**
- `item`: The raw DynamoDB item dictionary

**Use cases:**
- Validate required metadata fields
- Transform or normalize item structure before mapping
- Extract and store unmapped attributes
- Log or audit incoming data

#### 11.1.4 AfterToModel Hook

Invoked **after** property mapping and object construction during `ToModel`.

```csharp
static partial void AfterToModel(Dictionary<string, AttributeValue> item, ref T entity);
```

**Parameters:**
- `item`: The raw DynamoDB item dictionary (for reference)
- `entity`: The constructed and mapped entity (passed by `ref` for modification)

**Use cases:**
- Post-construction normalization
- Populate record type discriminators
- Hydrate computed properties
- Populate unmapped attribute bags
- Validate entity state after hydration

### 11.2 Hook Placement and Organization

#### 11.2.1 Same Partial Class
Hooks must be declared in the **same partial mapper class** as the mapping methods:

```csharp
[DynamoMapper(Convention = DynamoNamingConvention.CamelCase)]
public static partial class ProductMapper
{
    public static partial Dictionary<string, AttributeValue> FromModel(Product source);
    public static partial Product ToModel(Dictionary<string, AttributeValue> item);

    // Hooks defined in the same partial class
    static partial void AfterFromModel(Product source, Dictionary<string, AttributeValue> item)
    {
        item["pk"] = new AttributeValue { S = $"PRODUCT#{source.ProductId}" };
        item["sk"] = new AttributeValue { S = $"METADATA" };
        item["recordType"] = new AttributeValue { S = "Product" };
    }
}
```

#### 11.2.2 Separate Files for Organization
Hooks may be placed in **separate partial class files** for better organization:

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
    static partial void AfterFromModel(Product source, Dictionary<string, AttributeValue> item)
    {
        item["pk"] = new AttributeValue { S = $"PRODUCT#{source.ProductId}" };
        item["sk"] = new AttributeValue { S = $"METADATA" };
        item["recordType"] = new AttributeValue { S = "Product" };
    }

    static partial void AfterToModel(Dictionary<string, AttributeValue> item, ref Product entity)
    {
        // Populate additional state
    }
}
```

### 11.3 How Hooks Are Invoked

#### 11.3.1 Unconditional Invocation
The generated code **always invokes hooks unconditionally**, regardless of whether they are implemented:

```csharp
// Generated FromModel implementation
public static partial Dictionary<string, AttributeValue> FromModel(Product source)
{
    var item = new Dictionary<string, AttributeValue>(capacity: 5);

    BeforeFromModel(source, item); // Always invoked

    // Property mapping...
    item["productId"] = new AttributeValue { S = source.ProductId.ToString() };
    item["name"] = new AttributeValue { S = source.Name };

    AfterFromModel(source, item); // Always invoked

    return item;
}
```

#### 11.3.2 Partial Void Behavior
Unimplemented hooks **compile away** due to `partial void` semantics:

- If a hook is **not implemented**, the C# compiler removes the call site
- No runtime overhead for unused hooks
- No null checks or conditionals required

### 11.4 Common Use Cases

#### 11.4.1 Single-Table PK/SK Composition

```csharp
static partial void AfterFromModel(Order order, Dictionary<string, AttributeValue> item)
{
    item["pk"] = new AttributeValue { S = $"CUSTOMER#{order.CustomerId}" };
    item["sk"] = new AttributeValue { S = $"ORDER#{order.OrderId}" };
    item["recordType"] = new AttributeValue { S = "Order" };
}
```

#### 11.4.2 Record Type Discrimination

```csharp
static partial void AfterFromModel(Customer customer, Dictionary<string, AttributeValue> item)
{
    item["entityType"] = new AttributeValue { S = nameof(Customer) };
}

static partial void AfterToModel(Dictionary<string, AttributeValue> item, ref Customer entity)
{
    // Validate entity type on read
    if (item.TryGetValue("entityType", out var typeAttr) && typeAttr.S != nameof(Customer))
    {
        throw new InvalidOperationException($"Expected Customer, got {typeAttr.S}");
    }
}
```

#### 11.4.3 TTL Attributes

```csharp
static partial void AfterFromModel(Session session, Dictionary<string, AttributeValue> item)
{
    // Set TTL to 24 hours from now
    var ttl = DateTimeOffset.UtcNow.AddHours(24).ToUnixTimeSeconds();
    item["ttl"] = new AttributeValue { N = ttl.ToString() };
}
```

#### 11.4.4 Unmapped Attribute Bags

```csharp
public class Product
{
    public Guid ProductId { get; set; }
    public string Name { get; set; }
    public Dictionary<string, AttributeValue>? AdditionalAttributes { get; set; }
}

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
    var mappedKeys = new HashSet<string> { "pk", "sk", "productId", "name" };
    entity.AdditionalAttributes = item
        .Where(kvp => !mappedKeys.Contains(kvp.Key))
        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
}
```

#### 11.4.5 Post-Hydration Normalization

```csharp
static partial void AfterToModel(Dictionary<string, AttributeValue> item, ref User entity)
{
    // Normalize email to lowercase
    entity.Email = entity.Email?.ToLowerInvariant();

    // Compute derived property
    entity.FullName = $"{entity.FirstName} {entity.LastName}";
}
```

### 11.5 Hook Execution Order

Hooks execute in a deterministic order:

**During FromModel:**

1. `BeforeFromModel(source, item)` - item is empty
2. Generated property mapping
3. `AfterFromModel(source, item)` - item is fully populated

**During ToModel:**

1. `BeforeToModel(item)` - item is unmodified
2. Generated property mapping and object construction
3. `AfterToModel(item, ref entity)` - entity is constructed and populated

### 11.6 Constraints and Best Practices

#### 11.6.1 Constraints
- Hooks must be `static partial void`
- Hooks must match exact signatures
- Hooks cannot return values
- Hooks cannot be async (Phase 1)

#### 11.6.2 Best Practices
- Keep hooks focused and single-purpose
- Avoid complex business logic in hooks
- Use hooks for DynamoDB-specific concerns (keys, types, TTLs)
- Consider separate files for complex hook implementations
- Document hook behavior for team clarity

### 11.7 Diagnostics
The generator validates hook signatures and emits diagnostics for:
- Incorrect hook signatures
- Non-static hook methods
- Mismatched parameter types

### 11.8 Cross-References
See also:
- [Customization Hooks Guide](../../docs/usage/customization-hooks.md)
- [Single-Table Design Patterns](../../docs/examples/single-table-design.md)

---

## 12. Diagnostics (Compile-Time)

### 12.1 Required Diagnostics
Generator must report diagnostics for:
- Missing mapper partial methods (signature mismatch)
- Duplicate field names (two properties map to same Dynamo field name)
- Unsupported property types (unless a converter is specified)
- Converter type mismatch (converter doesn’t implement correct interface)
- Kind override incompatible with property type
- Ignoring or mapping a non-existent member name (string mismatch)
- Both Ignore and Field config applied to same member

### 12.2 Diagnostic Design
- Diagnostics should include:
  - code (e.g., `DM0001`)
  - severity (warning/error)
  - actionable message
  - location pointing at the relevant attribute or method

Recommended severities:
- Errors for invalid configs that prevent generation
- Warnings for suspicious configs (e.g., mapping computed property via member name string)

---

## 13. Generator Implementation Requirements

### 13.1 Incremental Generator
Must be implemented as `IIncrementalGenerator`.

### 13.2 Incremental Inputs
Generator should be driven by:
- Mapper types annotated with `[DynamoMapper]`
- Partial methods on those types with recognized signatures
- Attributes on those methods (`DynamoField`, `DynamoIgnore`)

Avoid whole-compilation scans beyond necessary discovery.

### 13.3 Supported Signatures (Phase 1)
Must recognize:

- `public static partial Dictionary<string, AttributeValue> FromModel(T source);`
- `public static partial T ToModel(Dictionary<string, AttributeValue> item);`

> Optional: allow `IReadOnlyDictionary<string, AttributeValue>` as input for ToModel.

### 13.4 Thread Safety / Determinism
- Generator must be deterministic: same input → same output
- No reliance on wall-clock time, random, etc.

---

## 14. Runtime Helpers (DynamoMapper.Runtime)

### 14.1 AttributeValue Extensions (Consumer-Facing)
Provide extension methods similar to:
- `GetString(key)`
- `GetNullableString(key)`
- `GetInt(key)`
- `GetGuid(key)`
- `TryGet...`

Even if generator inlines parsing, these helpers are useful for:
- manual mapping
- debugging
- custom hooks

These helpers must:
- include clear exceptions
- use invariant culture for numbers
- validate expected AttributeValue kind where possible

### 14.2 Helper Types
- `DynamoMappingException`
- `DynamoNamingConvention` enum
- `DynamoKind` enum
- Converter interfaces

All public types include XML documentation comments.

---

## 15. Testing Requirements

### 15.1 Generator Tests
Use snapshot-style tests:
- input C# code
- generated output verification
- diagnostic verification

Test cases must include:
- Default convention mapping
- Renamed field mapping
- Required missing field (runtime path or generated guard logic)
- Converter mapping
- Ignore computed properties
- Duplicate field name diagnostics
- Unsupported type diagnostics

### 15.2 Runtime Tests
Test `AttributeValue` extension helpers:
- correct parsing
- throws with good messages
- null/empty policies

---

## 16. Documentation Requirements (Phase 1)

DynamoMapper documentation is split between a **concise README** for quick onboarding and **GitHub Pages** for comprehensive reference material.


### 16.1 README.md (Quick Start)

The README is intentionally brief and focused on first-use success. It should assume readers will move to GitHub Pages for deeper details.

**README.md must include:**
- What is DynamoMapper?
- Install instructions:
  - `DynamoMapper` vs `DynamoMapper.Runtime`
- Quick start example
- Configuration reference (attributes)
- Supported types
- Converters
- Hooks
- Single-table patterns (recommended usage)

### 16.2 GitHub Pages Documentation

GitHub Pages is the primary long-form documentation site for DynamoMapper and must be kept in sync with released functionality.

**GitHub Pages must include:**

- Conceptual overview (why DynamoMapper exists)
- Installation guide (`DynamoMapper` vs `DynamoMapper.Runtime`)
- Attribute reference (full API docs)
- Naming conventions and defaults
- Required vs optional behavior
- Converter authoring guide
- Customization hooks (`BeforeFromModel`, `AfterToModel`, etc.)
- Diagnostics reference (error codes and meanings)
- Single-table DynamoDB patterns and best practices
- Migration guide from manual mapping or DynamoDBContext

GitHub Pages should be versioned logically (by major/minor release) but does not need strict per-version hosting initially.

### 16.3 Examples

Include at least one “single-table style” example:

- Add pk/sk via `BeforeFromModel`
- Add `recordType`
- Omit optional fields

---

## 17. Non-Goals (Explicitly Out of Scope for Phase 1)

Phase 1 does **not** need:
- DSL configuration (Phase 2)
- Nested maps (`M`) for complex objects
- Lists and sets (`L`, `SS`, `NS`, `BS`)
- UpdateExpression generation
- Projection/partial hydration generation
- Query builders
- Bidirectional “diff” / patch generation

---

## 18. Phase 2 Preview (DSL)
Phase 2 will introduce an optional fluent DSL for configuration while retaining attribute support. This phase will focus on:
- strongly-typed property selection (`x => x.OwnerId`)
- centralized configuration without many attributes
- constraints to keep generation deterministic and fast

> Phase 2 requirements will be authored separately.

---

## Appendix A – Suggested Attribute API (Draft)

> Names are suggestions; adjust to taste before publishing.

- `DynamoMapperAttribute`
- `DynamoFieldAttribute`
- `DynamoIgnoreAttribute`
- `DynamoNamingConvention`
- `DynamoKind`
- `DynamoMappingException`
- `IDynamoConverter<T>`

---

## Appendix B – Example Hook for Single-Table Keys

```csharp
public static partial class JediCharacterMapper
{
    static partial void BeforeFromModel(JediCharacter source, Dictionary<string, AttributeValue> item)
    {
        // Single-table keys + type discriminator 🧠
        item["pk"] = new AttributeValue { S = source.Pk };
        item["sk"] = new AttributeValue { S = source.Sk };
        item["recordType"] = new AttributeValue { S = "character" };
    }
}
```

---

## Appendix C – Generated Code Style Expectations (Summary)
- Prefer `sealed` for runtime classes where applicable
- Always include XML doc comments for public runtime API
- Avoid breaking changes in public attribute contracts
- Use deterministic ordering for emitted properties (source order or alpha, but consistent)

---

**End of Phase 1 Requirements** ✅
