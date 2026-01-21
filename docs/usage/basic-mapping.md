# Basic Mapping

This page covers the default mapping behavior of DynamoMapper and how the generator creates
instances during `FromItem`.

## Supported Model Shapes

DynamoMapper supports these common patterns:

- **Classes with settable properties** (`set`) and/or `init` setters
- **Classes with regular constructors** (the generator can choose a constructor automatically)
- **Classes with get-only properties**, when a constructor can populate them
- **Records / record structs** with primary constructors

The mapper itself remains the same:

```csharp
using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;
using DynamoMapper.Runtime;

namespace MyApp.Data;

[DynamoMapper]
public static partial class PersonMapper
{
    public static partial Dictionary<string, AttributeValue> ToItem(Person source);

    public static partial Person FromItem(Dictionary<string, AttributeValue> item);
}
```

## Object Construction During `FromItem`

When generating `FromItem`, DynamoMapper chooses between:

- **Property-based construction**: `new T { Prop = ..., ... }` plus optional post-construction
  assignments.
- **Constructor-based construction**: `new T(arg1, arg2, ...)` (optionally combined with an object
  initializer for settable/`init` properties).

## Nested Object Mapping

DynamoMapper supports nested objects and nested collections when the nested types are mappable.

### How Nested Mapping Is Chosen

For each nested object property, the generator uses this decision order:

1. **Dot-notation overrides**: if there are overrides for the nested path, inline mapping is used.
2. **Mapper-based**: if a mapper exists for the nested type and it defines the required direction
   (`ToItem` and/or `FromItem`), the nested mapper is used.
3. **Inline mapping**: otherwise, the nested type is inlined into the parent mapper.

### Nested Collections

Collections with nested element types are supported for:

- `List<T>` / `IEnumerable<T>` and arrays (`T[]`)
- `Dictionary<string, T>`

Nested collections of sets (SS/NS/BS) are not supported.

### Cycles

Nested object graphs cannot contain cycles. Cycles emit `DM0006`.

### Example

```csharp
public class Order
{
    public string Id { get; set; }
    public Address ShippingAddress { get; set; }
    public List<LineItem> Items { get; set; }
}

public class Address
{
    public string Line1 { get; set; }
    public string City { get; set; }
}
```

See `examples/DynamoMapper.Nested` for a complete example.

## Constructor Mapping Rules (`FromItem`)

Constructor selection is deterministic and follows these priorities.

### 1) When Constructor Selection Runs

Constructor selection is only evaluated when the mapper defines a `FromItem(...)` method.

### 2) Selection Priority

1. **Explicit constructor wins**

   If exactly one constructor is marked with `[DynamoMapperConstructor]`, DynamoMapper uses that
   constructor.

   If multiple constructors are marked, DynamoMapper emits diagnostic `DM0103`.

2. **No parameterless constructor**

   If the type has no parameterless constructor, DynamoMapper must use a non-parameterless
   constructor and selects the constructor with the most parameters.

3. **Parameterless constructor exists (prefer property initialization when possible)**

   If a parameterless constructor exists and all relevant properties can be populated via setters
   (`set` or `init`), DynamoMapper uses property-based construction (`new T { ... }`).

   Getter-only properties only force constructor-based deserialization if there is a constructor
   parameter that can populate them.

4. **Otherwise, use the constructor with the most parameters**

   This typically happens when the model has one or more getter-only properties that can be
   populated via constructor parameters.

### 3) Parameter Matching

Constructor parameters are matched to properties by a case-insensitive name comparison (e.g.
`firstName` matches `FirstName`).

!!! note

    Getter-only and computed properties have different behavior depending on mapping direction:

    - **ToItem (model → item):** included if the property has a getter and its type is mappable.
    - **FromItem (item → model):** can only be populated if DynamoMapper can supply the value via a
      constructor parameter; otherwise it is ignored.

### 4) How Values Are Applied (Constructor Args vs Initializers)

When a constructor is selected:

- Properties matched to constructor parameters are emitted as named constructor arguments.
- Remaining settable/`init` properties are emitted in an object initializer.
- Some optional settable properties with default initializers may be assigned after construction to
  avoid overwriting defaults when the DynamoDB attribute is missing.

## Gotchas

- `[DynamoMapperConstructor]` can only be applied to one constructor; multiple will emit `DM0103`.
- Constructor parameter matching is based on the .NET property name (case-insensitive). It is not
  based on DynamoDB `AttributeName` overrides.
- Adding a constructor parameter that matches a previously computed/get-only property can change
  whether DynamoMapper uses constructor-based deserialization.
- If the selected constructor contains required parameters that do not correspond to mappable
  properties, the generated `FromItem` code may fail to compile because DynamoMapper cannot supply
  arguments.

## Examples

### Record With Primary Constructor

```csharp
public record Person(string FirstName, string LastName, int Age);
```

`FromItem` will construct the record using its primary constructor.

### Class With Get-Only Properties

If your class has get-only properties, DynamoMapper can deserialize using a constructor:

```csharp
using DynamoMapper.Runtime;

public class Person
{
    [DynamoMapperConstructor]
    public Person(string firstName, string lastName, int age)
    {
        FirstName = firstName;
        LastName = lastName;
        Age = age;
    }

    public string FirstName { get; }
    public string LastName { get; }
    public int Age { get; }
}
```

### Hybrid: Constructor + Object Initializer

If some values come from the constructor and others are settable/`init`, DynamoMapper can generate a
hybrid `FromItem`:

```csharp
public record Product(string Id, string Name)
{
    public decimal Price { get; set; }
    public int Quantity { get; init; }
}
```

Generated code will call the constructor for `Id`/`Name` and use an object initializer for `Price`
and `Quantity`.

See `examples/DynamoMapper.MapperConstructor` for a working sample.
