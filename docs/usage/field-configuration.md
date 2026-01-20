# Field Configuration

Field configuration is handled through `[DynamoField]` attributes on the mapper class. These
attributes target model properties and allow you to override defaults without touching your domain
models.

## Basic Usage

```csharp
using DynamoMapper.Runtime;

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
