# Field Configuration

Field configuration is handled through `[DynamoField]` attributes on the mapper class. These
attributes target model properties and allow you to override defaults without touching your domain
models.

## Basic Usage

```csharp
using LayeredCraft.DynamoMapper.Runtime;

[DynamoMapper]
[DynamoField(nameof(Product.Name), AttributeName = "productName", Required = true)]
[DynamoField(nameof(Product.Description), OmitIfNull = true, OmitIfEmptyString = true)]
public static partial class ProductMapper
{
    public static partial Dictionary<string, AttributeValue> ToItem(Product source);
    public static partial Product FromItem(Dictionary<string, AttributeValue> item);
}
```

## Nested Property Overrides (Dot Notation)

Use dot notation to override nested properties without adding attributes to nested types:

```csharp
[DynamoMapper]
[DynamoField("ShippingAddress.Line1", AttributeName = "addr_line1")]
[DynamoField("ShippingAddress.City", AttributeName = "addr_city")]
public static partial class OrderMapper
{
    public static partial Dictionary<string, AttributeValue> ToItem(Order source);
    public static partial Order FromItem(Dictionary<string, AttributeValue> item);
}
```

Notes:
- Dot-notation overrides force inline mapping for the nested path.
- Invalid paths emit `DM0008`.

### Collection Element Members

The same dot-notation syntax works when the intermediate segment is a collection (`List<T>`, `T[]`,
`Dictionary<string, T>`, etc.). The path traverses into the **element type** of the collection:

```csharp
[DynamoMapper]
[DynamoField("Contacts.VerifiedAt", Format = "yyyy-MM-dd")]
[DynamoField("Contacts.Name", AttributeName = "contact_name")]
public static partial class CustomerMapper
{
    public static partial Dictionary<string, AttributeValue> ToItem(Customer source);
    public static partial Customer FromItem(Dictionary<string, AttributeValue> item);
}

public class Customer
{
    public string Id { get; set; }
    public List<CustomerContact> Contacts { get; set; }
}

public class CustomerContact
{
    public string Name { get; set; }
    public DateTime VerifiedAt { get; set; }
}
```

Notes:

- The override applies to every element in the collection — there is no per-index syntax.
- An invalid property name on the element type still emits `DM0008`.
- Dictionary value types are also supported: `"ProductMap.CreatedAt"` targets `CreatedAt` on
  the value type of `Dictionary<string, OrderItem>`.

## Supported Options

| Option | Description |
| --- | --- |
| `AttributeName` | Overrides the DynamoDB attribute name. |
| `Required` | Controls requiredness during `FromItem`. |
| `Kind` | Forces a specific `DynamoKind`. |
| `OmitIfNull` | Omits null values during `ToItem`. |
| `OmitIfEmptyString` | Omits empty strings during `ToItem`. |
| `ToMethod` | Uses a custom method to serialize a value. |
| `FromMethod` | Uses a custom method to deserialize a value. |
| `Format` | Overrides default format for date/time/enum conversions. |
