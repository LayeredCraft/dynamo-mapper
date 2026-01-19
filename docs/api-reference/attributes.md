# Attributes

DynamoMapper configuration is done via attributes on the **mapper class**.

## DynamoMapperAttribute

Marks a static partial class as a mapper and sets defaults.

```csharp
[DynamoMapper(
    Convention = DynamoNamingConvention.CamelCase,
    DefaultRequiredness = Requiredness.InferFromNullability,
    OmitNullStrings = true,
    OmitEmptyStrings = false,
    DateTimeFormat = "O",
    EnumFormat = "G")]
public static partial class OrderMapper
{
    public static partial Dictionary<string, AttributeValue> ToItem(Order source);
    public static partial Order FromItem(Dictionary<string, AttributeValue> item);
}
```

## DynamoFieldAttribute

Configures mapping for a specific member. Apply multiple times to the mapper class.

```csharp
[DynamoMapper]
[DynamoField(nameof(Order.Notes), OmitIfNull = true, OmitIfEmptyString = true)]
[DynamoField(nameof(Order.OrderId), AttributeName = "orderId")]
public static partial class OrderMapper
{
    public static partial Dictionary<string, AttributeValue> ToItem(Order source);
    public static partial Order FromItem(Dictionary<string, AttributeValue> item);
}
```

Properties:
- `MemberName` (ctor) - target member name
- `AttributeName` - DynamoDB attribute name override
- `Required` - requiredness override
- `Kind` - DynamoDB `DynamoKind` override
- `OmitIfNull` - omit when null
- `OmitIfEmptyString` - omit when empty string
- `ToMethod` / `FromMethod` - static conversion methods on the mapper class

## DynamoIgnoreAttribute

Skips a member in one or both directions.

```csharp
[DynamoMapper]
[DynamoIgnore(nameof(Order.InternalNotes), Ignore = IgnoreMapping.All)]
public static partial class OrderMapper
{
    public static partial Dictionary<string, AttributeValue> ToItem(Order source);
    public static partial Order FromItem(Dictionary<string, AttributeValue> item);
}
```

Properties:
- `MemberName` (ctor) - target member name
- `Ignore` - `IgnoreMapping.All`, `IgnoreMapping.FromModel`, or `IgnoreMapping.ToModel`

## DynamoMapperConstructorAttribute

Marks which constructor DynamoMapper should use when generating `FromItem` for a model type.

This attribute is applied to the **model's constructor**, not the mapper class.

```csharp
using DynamoMapper.Runtime;

public class User
{
    public User()
    {
        Id = string.Empty;
        Name = string.Empty;
    }

    [DynamoMapperConstructor]
    public User(string id, string name)
    {
        Id = id;
        Name = name;
    }

    public string Id { get; set; }
    public string Name { get; set; }
}
```

Rules:

- Only one constructor can be marked with `[DynamoMapperConstructor]`.
- If multiple are marked, DynamoMapper emits diagnostic `DM0103`.
