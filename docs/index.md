# DynamoMapper

**High-performance source generator for DynamoDB attribute mapping**

---

## Overview

DynamoMapper is a .NET incremental source generator that generates high-performance mapping code between **domain models** and **Amazon DynamoDB `AttributeValue` dictionaries**. Using compile-time code generation, it eliminates runtime reflection, reduces allocations, and provides type-safe mapping for single-table DynamoDB patterns.

### Why DynamoMapper?

Writing manual `ToItem()` and `FromItem()` methods for DynamoDB is repetitive and error-prone:

- Field naming inconsistencies (`UserId` vs `userId`)
- Attribute type mistakes (`S` vs `N`)
- Inconsistent null handling
- Boilerplate parsing code
- Hard-to-maintain single-table mapping logic

DynamoMapper solves these problems by generating clean, efficient mapping code at compile time, keeping your domain models free of persistence attributes while ensuring type-safe, high-performance DynamoDB operations.

## Quick Example

```csharp
// Domain model - clean and attribute-free
public class Product
{
    public string UserId { get; set; }
    public Guid ProductId { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string? Description { get; set; }
}

// Mapper - configuration lives here
[DynamoMapper(Convention = DynamoNamingConvention.CamelCase)]
public static partial class ProductMapper
{
    [DynamoField(nameof(Product.Description), OmitIfNullOrWhiteSpace = true)]
    public static partial Dictionary<string, AttributeValue> ToItem(Product source);
    public static partial Product FromItem(Dictionary<string, AttributeValue> item);
}

// Usage - simple and type-safe
var item = ProductMapper.ToItem(product);
var product = ProductMapper.FromItem(item);
```

See the [Quick Start Guide](getting-started/quick-start.md) for a complete tutorial.

## Key Features

- **Zero Runtime Overhead** - All mapping code generated at compile time
- **Type-Safe** - Catch errors at compile time with comprehensive diagnostics
- **Allocation-Free** - Efficient dictionary operations, no LINQ or unnecessary allocations
- **Clean Domain Models** - No attributes required on your domain classes
- **Convention-First** - Sensible defaults with selective overrides
- **Single-Table Friendly** - Built-in support for DynamoDB single-table patterns
- **Comprehensive Diagnostics** - Clear compile-time errors with actionable messages

## Getting Started

1. [Installation](getting-started/installation.md) - Set up DynamoMapper in your project
2. [Requirements](getting-started/requirements.md) - System requirements and dependencies
3. [Quick Start](getting-started/quick-start.md) - Build your first mapper in 5 minutes

## Documentation

- **[Core Concepts](core-concepts/how-it-works.md)** - Understand how DynamoMapper works
- **[Usage Guide](usage/basic-mapping.md)** - Learn how to use DynamoMapper features
- **[Examples](examples/index.md)** - Real-world scenarios and patterns
- **[Advanced Topics](advanced/performance.md)** - Performance, diagnostics, and testing
- **[API Reference](api-reference/attributes.md)** - Complete API documentation
- **[Roadmap](roadmap/phase-1.md)** - Current and planned features

## Support

- **Documentation**: [https://layeredcraft.github.io/dynamo-mapper/](https://layeredcraft.github.io/dynamo-mapper/)
- **GitHub**: [https://github.com/LayeredCraft/dynamo-mapper](https://github.com/LayeredCraft/dynamo-mapper)
- **NuGet**: [https://www.nuget.org/packages/DynamoMapper/](https://www.nuget.org/packages/DynamoMapper/)
