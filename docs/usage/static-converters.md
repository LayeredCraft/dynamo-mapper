---
title: Static Conversion Methods
description: Custom member conversion using the source model and item dictionary
---

# Static conversion methods

Configure custom conversion on the mapper class using `DynamoField.ToMethod` and
`DynamoField.FromMethod`.

The signatures use the whole model and item dictionary:

```csharp
static AttributeValue ToMethodName(TModel source);
static TProperty FromMethodName(Dictionary<string, AttributeValue> item);
```

`ToMethod` returns the value stored under the member's configured attribute name.
`FromMethod` returns the member value. It must read the correct key from the dictionary
itself. Both methods must be accessible static methods on the mapper class.

You may override one direction. The other direction uses normal mapping if the member
type is supported. For an unsupported type, supply custom conversion for each direction
you generate, or ignore that direction.

## Example

```csharp
using System.Collections.Generic;
using System.Globalization;
using Amazon.DynamoDBv2.Model;
using LayeredCraft.DynamoMapper.Runtime;

public sealed class Product
{
    public decimal Price { get; set; }
}

[DynamoMapper]
[DynamoField(nameof(Product.Price), AttributeName = "price_text",
    ToMethod = nameof(WritePrice), FromMethod = nameof(ReadPrice))]
public static partial class ProductMapper
{
    public static partial Dictionary<string, AttributeValue> ToItem(Product source);
    public static partial Product FromItem(Dictionary<string, AttributeValue> item);

    private static AttributeValue WritePrice(Product source) =>
        new() { S = source.Price.ToString(CultureInfo.InvariantCulture) };

    private static decimal ReadPrice(Dictionary<string, AttributeValue> item) =>
        decimal.Parse(item["price_text"].S, CultureInfo.InvariantCulture);
}
```

The generated write is equivalent to `item.Set("price_text", WritePrice(source))`.
The generated read assigns `ReadPrice(item)` to `Price`.

## Nulls, missing keys, and validation

Custom methods replace the normal conversion calls. Handle null values, missing keys,
requiredness, and formatting inside your methods. For optional members, use
`TryGetValue` and return an appropriate default when the key is absent.

Incorrect method names or signatures can produce ordinary C# compiler errors. There
is no dedicated converter diagnostic family in the current implementation.

For shared conversion logic, call a shared static helper from the mapper's methods.
Converter types such as `IDynamoConverter<T>` and fluent DSL configuration are not
implemented.

See [Attributes](../api-reference/attributes.md) for member configuration and
[Naming conventions](../core-concepts/naming-conventions.md) for key naming.
