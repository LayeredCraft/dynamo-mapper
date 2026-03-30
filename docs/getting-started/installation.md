# Installation

This guide walks you through installing LayeredCraft.DynamoMapper in your .NET project.

!!! note "Migrating from `DynamoMapper`"
    `LayeredCraft.DynamoMapper` replaces the original `DynamoMapper` NuGet package. If you are upgrading:

    1. Uninstall the old package: `dotnet remove package DynamoMapper`
    2. Install the new package: `dotnet add package LayeredCraft.DynamoMapper --prerelease`
    3. Update all `using LayeredCraft.DynamoMapper.Runtime;` statements to `using LayeredCraft.DynamoMapper.Runtime;`

## Prerequisites

- .NET SDK 8.0 or later
- C# 11 or later
- AWSSDK.DynamoDBv2 package (for AttributeValue types)

## Install via NuGet

Add the DynamoMapper package to your project:

```bash
dotnet add package LayeredCraft.DynamoMapper --prerelease
```

This installs both the source generator and runtime packages.

## Install the agent skill

If you want an agent to use the DynamoMapper skill from this repository, install it with:

```bash
npx skills add https://github.com/LayeredCraft/dynamo-mapper --skill dynamo-mapper
```

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
using LayeredCraft.DynamoMapper.Runtime;
using Amazon.DynamoDBv2.Model;

[DynamoMapper]
public static partial class TestMapper
{
    public static partial Dictionary<string, AttributeValue> ToItem(TestEntity source);
    public static partial TestEntity FromItem(Dictionary<string, AttributeValue> item);
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
