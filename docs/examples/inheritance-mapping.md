# Inheritance Mapping

By default, DynamoMapper only maps properties declared on the model type being mapped. It does not
map properties declared on base classes.

To include inherited properties, enable the mapper option:

```csharp
using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;
using DynamoMapper.Runtime;

namespace MyApp.Data;

[DynamoMapper(IncludeBaseClassProperties = true)]
public static partial class OrderMapper
{
    public static partial Dictionary<string, AttributeValue> ToItem(Order source);

    public static partial Order FromItem(Dictionary<string, AttributeValue> item);
}

public class BaseEntity
{
    public string Id { get; set; } = string.Empty;
}

public class Order : BaseEntity
{
    public string Name { get; set; } = string.Empty;
}
```

Behavior notes:

- When enabled, inherited properties also participate in nested inline mapping.
- If a derived type declares a property with the same name as an inherited property, the derived
  property wins.

See `examples/DynamoMapper.Inheritance` for a complete, runnable example showing both
`IncludeBaseClassProperties = false` (default) and `true`.
