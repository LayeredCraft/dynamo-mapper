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

### Constructor Selection Rules

Constructor selection is deterministic and follows these priorities:

1. If exactly one constructor is marked with `[DynamoMapperConstructor]`, use it.
2. If there is no parameterless constructor, use the constructor with the most parameters.
3. If a parameterless constructor exists and all relevant properties can be populated via setters,
   use property-based construction.
4. Otherwise, use the constructor with the most parameters.

Parameter names are matched to property names case-insensitively (e.g. `firstName` matches
`FirstName`).

!!! note

    Getter-only and computed properties have different behavior depending on mapping direction:

    - **ToItem (model → item):** they are included as long as they have a getter.
    - **FromItem (item → model):** they can only be populated if DynamoMapper can supply the value
      via a constructor parameter. Otherwise they are ignored.

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

See `examples/DynamoMapper.MapperConstructor` for a working sample.
