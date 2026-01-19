# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Instructions For Claude

### Querying Microsoft Documentation

When handling questions around how to work with native Microsoft technologies, such as C#, F#, ASP.NET Core, Microsoft.Extensions, NuGet, Entity Framework, the `dotnet` runtime - please use this tool for research purposes when dealing with specific / narrowly defined questions that may occur.

## Project Overview

DynamoMapper is a .NET incremental source generator that generates compile-time mapping code between domain models and Amazon DynamoDB AttributeValue dictionaries. It eliminates runtime reflection, reduces allocations, and provides type-safe mapping for single-table DynamoDB patterns.

**Current Status:**
- Version: 0.1.0-alpha (pre-release)
- Active Branch: feature/basic-mapper
- Phase: Phase 1 (attribute-based mapping)

## Common Commands

### Build
```bash
dotnet build
```

### Run Tests
```bash
# Run all tests across all target frameworks (net8.0, net9.0, net10.0)
dotnet test

# Run tests for a specific framework
# (recommended when working on a single test failure)
dotnet test --framework net8.0

# Run tests with verbose output
dotnet test --logger "console;verbosity=detailed"

# --- xUnit v3 + Microsoft.Testing.Platform (lowest-noise filtered runs) ---

# List available tests (discovery)
# Copy the fully-qualified test name from this output.
DOTNET_NOLOGO=1 dotnet test \
  --project test/LayeredCraft.DynamoMapper.Generators.Tests/LayeredCraft.DynamoMapper.Generators.Tests.csproj \
  -f net10.0 \
  -v q \
  --list-tests \
  --no-progress \
  --no-ansi

# Run a single test method (exact fully-qualified name)
DOTNET_NOLOGO=1 dotnet test \
  --project test/LayeredCraft.DynamoMapper.Generators.Tests/LayeredCraft.DynamoMapper.Generators.Tests.csproj \
  -f net10.0 \
  -v q \
  --filter-method "MyNamespace.MyTestClass.MyTestMethod" \
  --minimum-expected-tests 1 \
  --no-progress \
  --no-ansi

# Variants: class / namespace
DOTNET_NOLOGO=1 dotnet test \
  --project test/LayeredCraft.DynamoMapper.Generators.Tests/LayeredCraft.DynamoMapper.Generators.Tests.csproj \
  -f net10.0 \
  -v q \
  --filter-class "MyNamespace.MyTestClass" \
  --minimum-expected-tests 1 \
  --no-progress \
  --no-ansi

DOTNET_NOLOGO=1 dotnet test \
  --project test/LayeredCraft.DynamoMapper.Generators.Tests/LayeredCraft.DynamoMapper.Generators.Tests.csproj \
  -f net10.0 \
  -v q \
  --filter-namespace "MyNamespace.Tests" \
  --minimum-expected-tests 1 \
  --no-progress \
  --no-ansi
```

### Restore Dependencies
```bash
dotnet restore
```

### Build Examples
```bash
dotnet build examples/DynamoMapper.SimpleExample/DynamoMapper.SimpleExample.csproj
```

### Run Example
```bash
dotnet run --project examples/DynamoMapper.SimpleExample/DynamoMapper.SimpleExample.csproj
```

## Architecture Overview

### Project Structure

**Source Generators** (`src/LayeredCraft.DynamoMapper.Generators/`):
- Implements `IIncrementalGenerator` for Roslyn-based code generation
- Uses Scriban templates (`Templates/Mapper.scriban`) to generate mapper code
- Performs compile-time analysis and validation with diagnostics

**Runtime API** (`src/LayeredCraft.DynamoMapper.Runtime/`):
- Provides `[DynamoMapper]` attribute for marking mapper classes
- Contains `AttributeValueExtensions` for manual mapping and hook usage
- No dependencies on the generator at runtime

**Tests**:
- `LayeredCraft.DynamoMapper.Generators.Tests`: Snapshot testing with Verify.SourceGenerators
- `LayeredCraft.DynamoMapper.Runtime.Tests`: Unit tests for runtime helpers
- `LayeredCraft.DynamoMapper.TestKit`: Shared test utilities

### Source Generation Pipeline

1. **Discovery**: `ForAttributeWithMetadataName()` finds classes marked with `[DynamoMapper]`
2. **Analysis**: `MapperSyntaxProvider.Transformer()` extracts mapper and model metadata
3. **Type Resolution**: Analyzes properties to determine AttributeValue mappings
4. **Code Generation**: `MapperEmitter.Generate()` renders Scriban template
5. **Output**: Generates `{MapperClassName}.g.cs` with `ToItem` and `FromItem` methods

### Key Components

**DynamoMapperGenerator.cs**: Orchestrates the entire incremental generation pipeline

**MapperClassInfo**: Metadata about the mapper class (name, namespace, method signatures)

**ModelClassInfo**: Metadata about the domain model (properties, types, mapping logic)

**AttributeValueExtensions**: Runtime helpers for getting/setting AttributeValue fields
- `GetString()`, `GetInt()`, `GetGuid()`, `GetDateTime()`, etc.
- Handles nullable versions of all types
- Uses InvariantCulture for numeric conversions
- Supports enums via `GetEnum<T>()` and `SetEnum<T>()`

**WellKnownTypes**: Caches and resolves commonly-used types from Roslyn compilation

**Mapper.scriban**: Scriban template that generates both `ToItem` and `FromItem` method implementations

## Supported Types (Phase 1)

Primitives: `string`, `bool`, `byte`, `int`, `long`, `float`, `double`, `decimal`
Date/Time: `DateTime`, `DateTimeOffset`, `TimeSpan`
Other: `Guid`, `enum`
Nullable: All nullable versions of the above

## Key Design Patterns

### Incremental Source Generation
- Uses Roslyn's incremental API for efficient regeneration
- Only regenerates code when mapper or model changes
- Tracking names help with debugging

### Extension Methods
- Generated mappers use extension methods on `Dictionary<string, AttributeValue>`
- Runtime package provides standalone extension methods for manual use
- All conversions use `CultureInfo.InvariantCulture`

### Functional Error Handling
- `Result<T>` type for success/error paths
- `DiagnosticInfo` wraps Roslyn diagnostics
- Compile-time errors prevent code generation

### Convention-First Configuration
- Minimal attributes required (just `[DynamoMapper]`)
- Properties auto-mapped by convention
- Customization via method-level attributes (Phase 1) or DSL (Phase 2, future)

## Naming Conventions

**Method Prefixes**:
- "To" prefix: Domain model → DynamoDB (e.g., `ToItem`, `ToProduct`)
- "From" prefix: DynamoDB → Domain model (e.g., `FromItem`, `FromProduct`)

**Generator Tracking Names** (in `TrackingName.cs`): Used for Roslyn incremental generation debugging

## Customization Hooks (Phase 1)

Mappers support four lifecycle hooks as `static partial void` methods:

- `BeforeToItem(T source, Dictionary<string, AttributeValue> item)`: Before property mapping
- `AfterToItem(T source, Dictionary<string, AttributeValue> item)`: After property mapping (add pk/sk here)
- `BeforeFromItem(Dictionary<string, AttributeValue> item)`: Before deserialization
- `AfterFromItem(Dictionary<string, AttributeValue> item, ref T entity)`: After object construction

**Common Use Cases**:
- Adding single-table keys (pk/sk) in `AfterToItem`
- Adding discriminator fields (`recordType`, `entityType`)
- Adding TTL attributes
- Capturing unmapped attributes in an attribute bag

## Diagnostics

Current diagnostics (see `src/LayeredCraft.DynamoMapper.Generators/Diagnostics/Diagnostics.cs`):
- `DM0001`: Type cannot be mapped to an AttributeValue
- `DM0002`: Type cannot be mapped from an AttributeValue

More diagnostics planned per Phase 1 requirements.

## Build System

**Framework Targets**:
- Generators: `netstandard2.0` (broad compatibility)
- Tests: `net8.0`, `net9.0`, `net10.0` (multi-targeted)
- Examples: `net10.0`

**Package Management**:
- Central package versions in `Directory.Packages.props`
- Global build props in `Directory.Build.props`
- Version: `0.1.0-alpha` (set in `Directory.Build.props`)

**Key Dependencies**:
- `AWSSDK.DynamoDBv2` (4.0.10.4): AttributeValue types
- `Microsoft.CodeAnalysis` (5.0.0): Roslyn source generation
- `Scriban` (6.5.2): Template engine for code generation
- `xunit.v3` (3.2.1): Testing framework
- `Verify.SourceGenerators` (2.5.0): Snapshot testing

## Testing Strategy

**Snapshot Testing**: Use `Verify.SourceGenerators` to validate generated code matches expected syntax

**Test Organization**:
- One test per mapping scenario (basic types, nullable types, enums, etc.)
- Snapshots stored in `/test/.../Snapshots/`
- Multi-targeted tests ensure compatibility across .NET versions

**Running Snapshot Tests**:
- On failure, review the `.received.txt` files
- Update snapshots by accepting new baselines if changes are intentional

## Development Workflow

1. Make changes to generator logic in `src/LayeredCraft.DynamoMapper.Generators/`
2. Run `dotnet build` to compile
3. Run `dotnet test` to verify snapshot tests pass
4. Check generated code in `examples/DynamoMapper.SimpleExample/obj/` for manual validation
5. Update snapshots if generator output intentionally changed

## Phase 1 Requirements

Currently implementing Phase 1 (attribute-based mapping). See `docs/roadmap/phase-1.md` for full specification.

**Phase 1 Scope**:
- Attribute-based configuration on mapper classes (no attributes on domain models)
- Convention-first mapping with selective overrides
- High-performance, zero-reflection runtime
- Support for scalar types (no nested objects or collections in Phase 1)
- Customization hooks for single-table patterns
- Comprehensive compile-time diagnostics

**Phase 2 (Future)**:
- Fluent DSL configuration
- Nested objects and collections
- Advanced customization

## Documentation

- **README.md**: Quick start guide
- **docs/**: Comprehensive MkDocs-based documentation
  - Getting Started, Core Concepts, Usage Guide
  - Examples, Advanced Topics, API Reference
  - Roadmap (Phase 1 and Phase 2)
- **GitHub Pages**: https://layeredcraft.github.io/dynamo-mapper/

## Important Notes

- **Domain models stay clean**: No attributes required on POCOs
- **DynamoDB-specific**: Not a general-purpose object mapper (only maps to/from `Dictionary<string, AttributeValue>`)
- **Single-table patterns**: Designed for single-table DynamoDB with pk/sk composition via hooks
- **Culture-safe**: All numeric conversions use `InvariantCulture`
- **Deterministic**: Generator output must be deterministic for incremental builds
