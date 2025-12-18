# Requirements

## System Requirements

- **.NET SDK**: 8.0 or later
- **C# Language Version**: 11 or later
- **IDE**: Visual Studio 2022 17.3+, Rider 2022.2+, or VS Code with C# extension

## NuGet Dependencies

DynamoMapper requires:

- `AWSSDK.DynamoDBv2` - For AttributeValue and DynamoDB types
- `Microsoft.CodeAnalysis.CSharp` - Source generator infrastructure (automatically included)

## Target Frameworks

DynamoMapper packages target:

- **Runtime package** (`DynamoMapper.Runtime`): .NET Standard 2.0
- **Generator package** (`DynamoMapper`): .NET Standard 2.0

This means you can use DynamoMapper in projects targeting:

- .NET 8.0+
- .NET Framework 4.7.2+ (with C# 11 compiler)

## AWS SDK Version

DynamoMapper is tested with AWS SDK for .NET v3.7+. Earlier versions may work but are not officially supported.

## Next Steps

- [Installation](installation.md) - Install DynamoMapper
- [Quick Start](quick-start.md) - Build your first mapper
