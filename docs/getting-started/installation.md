# Installation

This guide walks you through installing DynamoMapper in your .NET project.

## Prerequisites

- .NET SDK 8.0 or later
- C# 11 or later
- AWSSDK.DynamoDBv2 package (for AttributeValue types)

## Install via NuGet

Add the DynamoMapper package to your project:

```bash
dotnet add package DynamoMapper --prerelease
```

This installs both the source generator and runtime packages.

## Configure C# Version

Ensure your project uses C# 11 or later in your `.csproj` file:

```xml
<PropertyGroup>
  <LangVersion>11</LangVersion>
  <!-- or -->
  <LangVersion>latest</LangVersion>
</PropertyGroup>
```

## Verify Installation

Create a simple test mapper to verify installation:

```csharp
using DynamoMapper.Attributes;
using Amazon.DynamoDBv2.Model;

[DynamoMapper]
public static partial class TestMapper
{
    public static partial Dictionary<string, AttributeValue> FromModel(TestEntity source);
    public static partial TestEntity ToModel(Dictionary<string, AttributeValue> item);
}

public class TestEntity
{
    public string Id { get; set; }
}
```

Build your project. If the source generator is working correctly, you'll see generated code in your IDE.

## Next Steps

- [Requirements](requirements.md) - Understand system requirements
- [Quick Start](quick-start.md) - Build your first real mapper
