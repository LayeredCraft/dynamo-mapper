# Naming conventions

DynamoMapper uses `DynamoNamingConvention.CamelCase` by default. Configure a different
convention on the mapper class with `DynamoMapperAttribute.Convention`.

| Convention | .NET property | DynamoDB attribute |
| --- | --- | --- |
| `Exact` | `ProductId` | `ProductId` |
| `CamelCase` | `ProductId` | `productId` |
| `PascalCase` | `productId` | `ProductId` |
| `SnakeCase` | `ProductId` | `product_id` |
| `KebabCase` | `ProductId` | `product-id` |

```csharp
using Amazon.DynamoDBv2.Model;
using LayeredCraft.DynamoMapper.Runtime;

public sealed class Product
{
    public string ProductId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}

[DynamoMapper(Convention = DynamoNamingConvention.SnakeCase)]
[DynamoField(nameof(Product.ProductId), AttributeName = "pk")]
public static partial class ProductMapper
{
    public static partial Dictionary<string, AttributeValue> ToItem(Product source);
    public static partial Product FromItem(Dictionary<string, AttributeValue> item);
}
```

This mapper writes and reads `pk` and `display_name`. `AttributeName` overrides the
convention for that member in both directions. Member selectors such as
`nameof(Product.ProductId)` refer to .NET property names, not DynamoDB keys.

Naming conversions use Humanizer's `Camelize`, `Pascalize`, `Underscore`, and
`Kebaberize` methods. Use an explicit `AttributeName` when an acronym or legacy key
needs a particular spelling. Changing a convention changes the keys the mapper reads
and writes; it does not rename attributes already stored in DynamoDB.

Mapping methods may have names such as `ToItem`/`FromItem` or `ToAttributes`/`FromAttributes`.
The generator recognizes unimplemented partial methods with `To`/`From` prefixes and
matching model/dictionary signatures. Lifecycle hook names remain exactly `BeforeToItem`,
`AfterToItem`, `BeforeFromItem`, and `AfterFromItem` regardless of mapping method names.
