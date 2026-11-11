# Requirements

## System Requirements

- **.NET SDK**: 8.0 or later
- **C# Language Version**: 11 or later
- **IDE**: Visual Studio 2022 17.3+, Rider 2022.2+, or VS Code with C# extension

## NuGet Dependencies

DynamoMapper requires:

- `AWSSDK.DynamoDBv2` - For AttributeValue and DynamoDB types
The generator uses `Microsoft.CodeAnalysis.CSharp` at build time. Consumers need a
compiler compatible with the generator's Roslyn version; targeting an older framework
does not make an older compiler compatible. For the current source checkout, use the
SDK in `global.json` and the dependency versions in `Directory.Packages.props`.

## Target Frameworks

DynamoMapper packages target:

- **NuGet package** (`LayeredCraft.DynamoMapper`): includes the generator and runtime
  assembly, both targeting .NET Standard 2.0.
- **Runtime source project** (`LayeredCraft.DynamoMapper.Runtime`): also targets
  .NET 8, 9, and 10, plus .NET 11 when built with the corresponding SDK. It is not
  packed separately by the current project configuration.

This means you can use DynamoMapper in projects targeting:

- .NET 8.0+
- .NET Framework 4.7.2+ (with C# 11 compiler)

## DateOnly and TimeOnly

These types require the runtime helpers compiled for .NET 8 or later. The current
NuGet packing configuration includes only the .NET Standard 2.0 runtime assembly,
which excludes those helpers. A modern application target alone does not enable
this support. Use custom conversions when those helpers are unavailable.

## AWS SDK Version

The current repository uses AWSSDK.DynamoDBv2 4.x. See `Directory.Packages.props` for
the exact version used by the build and tests. Compatibility with AWS SDK 3.x is not
validated by the current test configuration.

## Next Steps

- [Installation](installation.md) - Install DynamoMapper
- [Quick Start](quick-start.md) - Build your first mapper
