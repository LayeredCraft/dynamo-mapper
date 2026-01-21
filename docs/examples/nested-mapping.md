# Nested Mapping Example

The `examples/DynamoMapper.Nested` project demonstrates:

- Nested object mapping (inline and mapper-based)
- Nested collections (`List<T>`, arrays, `Dictionary<string, T>`)
- Dot-notation overrides for nested paths
- Cycle detection diagnostics

## Example: Nested Objects

```csharp
using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;
using DynamoMapper.Runtime;

[DynamoMapper]
public static partial class OrderMapper
{
    public static partial Dictionary<string, AttributeValue> ToItem(Order source);
    public static partial Order FromItem(Dictionary<string, AttributeValue> item);
}

public class Order
{
    public string Id { get; set; }
    public Address ShippingAddress { get; set; }
}

public class Address
{
    public string Line1 { get; set; }
    public string City { get; set; }
}
```

## Example: Nested Collection

```csharp
public class Catalog
{
    public string Id { get; set; }
    public Dictionary<string, Product> Products { get; set; }
}

public class Product
{
    public string Name { get; set; }
    public decimal Price { get; set; }
}
```

## Dot-Notation Overrides

```csharp
[DynamoMapper]
[DynamoField("ShippingAddress.Line1", AttributeName = "addr_line1")]
[DynamoField("ShippingAddress.City", AttributeName = "addr_city")]
public static partial class OrderMapper { }
```

See `examples/DynamoMapper.Nested/Program.cs` for the full walkthrough.
