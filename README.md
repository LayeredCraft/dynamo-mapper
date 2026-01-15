# DynamoMapper

**High-performance source generator for DynamoDB attribute mapping**

[![NuGet](https://img.shields.io/nuget/v/DynamoMapper.svg)](https://www.nuget.org/packages/DynamoMapper/)
[![Build Status](https://github.com/LayeredCraft/dynamo-mapper/actions/workflows/build.yaml/badge.svg)](https://github.com/LayeredCraft/dynamo-mapper/actions/workflows/build.yaml)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![Documentation](https://img.shields.io/badge/docs-latest-blue)](https://layeredcraft.github.io/dynamo-mapper/)

---

DynamoMapper is a .NET incremental source generator that generates high-performance mapping code between **domain models** and **Amazon DynamoDB `AttributeValue` dictionaries**. Using compile-time code generation, it eliminates runtime reflection, reduces allocations, and provides type-safe mapping for single-table DynamoDB patterns.

**Why DynamoMapper?**
- ⚡ **Zero reflection overhead** - All mapping code generated at compile time
- 🎯 **Type-safe** - Catches configuration errors at compile time with diagnostics
- 🚀 **Allocation-free** - Uses efficient dictionary operations, no LINQ or unnecessary allocations
- 🔧 **Simple** - Convention-first with minimal attribute configuration
- 🌐 **Single-table friendly** - Designed for single-table DynamoDB patterns with customization hooks
- 📦 **Clean domain models** - No attributes on your domain classes required

📚 **[View Full Documentation](https://layeredcraft.github.io/dynamo-mapper/)**

## Installation

Install the NuGet package:

```bash
dotnet add package DynamoMapper --prerelease
```

Ensure your project uses C# 11 or later:

```xml
<PropertyGroup>
  <LangVersion>11</LangVersion>
  <!-- or <LangVersion>latest</LangVersion> -->
</PropertyGroup>
```

## Quick Start

### 1. Define your domain model

```csharp
public class Product
{
    public string UserId { get; set; }
    public Guid ProductId { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public ProductStatus Status { get; set; }
}
```

### 2. Create a mapper class

```csharp
using DynamoMapper.Attributes;
using Amazon.DynamoDBv2.Model;

namespace MyApp.Data;

[DynamoMapper(Convention = DynamoNamingConvention.CamelCase)]
public static partial class ProductMapper
{
    [DynamoField(nameof(Product.Description), OmitIfNullOrWhiteSpace = true)]
    public static partial Dictionary<string, AttributeValue> FromModel(Product source);

    public static partial Product ToModel(Dictionary<string, AttributeValue> item);
}
```

### 3. That's it!

At compile time, DynamoMapper generates the mapping code. Use it like this:

```csharp
var product = new Product
{
    UserId = "user-123",
    ProductId = Guid.NewGuid(),
    Name = "Laptop",
    Price = 999.99m,
    Status = ProductStatus.Available
};

// Convert to DynamoDB item
var item = ProductMapper.FromModel(product);
await dynamoDbClient.PutItemAsync(new PutItemRequest
{
    TableName = "MyTable",
    Item = item
});

// Convert from DynamoDB item
var getResponse = await dynamoDbClient.GetItemAsync(new GetItemRequest
{
    TableName = "MyTable",
    Key = new Dictionary<string, AttributeValue>
    {
        ["pk"] = new AttributeValue { S = $"USER#{product.UserId}" },
        ["sk"] = new AttributeValue { S = $"PRODUCT#{product.ProductId}" }
    }
});
var retrievedProduct = ProductMapper.ToModel(getResponse.Item);
```

For more examples including single-table patterns and custom converters, see the [Quick Start Guide](https://layeredcraft.github.io/dynamo-mapper/getting-started/quick-start/).

## Key Features

- **Attribute-Based Configuration** (Phase 1): Configure mapping via `[DynamoField]` and `[DynamoIgnore]` attributes on mapper methods
- **Fluent DSL Configuration** (Phase 2): Optional strongly-typed DSL for configuration
- **Naming Conventions**: CamelCase, SnakeCase, or Exact field naming
- **Required Field Validation**: Compile-time and runtime validation of required fields
- **Omission Policies**: Flexible null/empty value omission strategies
- **Custom Converters**: Type-safe converter pattern for custom serialization
- **Customization Hooks**: Before/After hooks for injecting pk/sk and custom logic
- **Single-Table Support**: Built for single-table DynamoDB design patterns
- **Supported Types**: string, numeric types, bool, Guid, DateTime, DateTimeOffset, TimeSpan, enums
- **Comprehensive Diagnostics**: Clear compile-time errors with actionable messages
- **Zero Configuration**: Sensible defaults for most use cases

Learn more in the [Core Concepts](https://layeredcraft.github.io/dynamo-mapper/core-concepts/how-it-works/) documentation.

## Requirements

- C# 11+ (for partial method generation)
- .NET Standard 2.0+ (package targets)
- AWSSDK.DynamoDBv2 (for AttributeValue types)

See the [Requirements](https://layeredcraft.github.io/dynamo-mapper/getting-started/requirements/) page for full details.

## Documentation

📖 **[Full Documentation](https://layeredcraft.github.io/dynamo-mapper/)** - Comprehensive guides and API reference

Key sections:
- [Installation](https://layeredcraft.github.io/dynamo-mapper/getting-started/installation/) - Get started with DynamoMapper
- [Quick Start](https://layeredcraft.github.io/dynamo-mapper/getting-started/quick-start/) - Your first mapper in 5 minutes
- [Core Concepts](https://layeredcraft.github.io/dynamo-mapper/core-concepts/how-it-works/) - Understand how it works
- [Usage Guide](https://layeredcraft.github.io/dynamo-mapper/usage/basic-mapping/) - Detailed usage patterns
- [Examples](https://layeredcraft.github.io/dynamo-mapper/examples/) - Real-world DynamoDB scenarios
- [Roadmap](https://layeredcraft.github.io/dynamo-mapper/roadmap/phase-1/) - Phase 1 and Phase 2 plans

## Contributing

Contributions are welcome! See our [Contributing Guide](https://layeredcraft.github.io/dynamo-mapper/contributing/) for details.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

**Made with ⚡ by the LayeredCraft team**