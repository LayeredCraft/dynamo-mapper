# DynamoMapper Agent Guidelines

## Build, Test, and Development Commands

### Build Commands
```bash
# Build entire solution
dotnet build

# Build specific project
dotnet build src/LayeredCraft.DynamoMapper.Generators/LayeredCraft.DynamoMapper.Generators.csproj

# Build examples
dotnet build examples/DynamoMapper.SimpleExample/DynamoMapper.SimpleExample.csproj
```

### Test Commands
```bash
# Run all tests across all target frameworks (net8.0, net9.0, net10.0)
dotnet test

# Run tests for a specific framework
# (recommended when working on a single test failure)
dotnet test --framework net8.0

# Run tests with verbose output
dotnet test --logger "console;verbosity=detailed"

# Run tests in a specific project (avoid running other test projects)
dotnet test --project test/LayeredCraft.DynamoMapper.Generators.Tests/LayeredCraft.DynamoMapper.Generators.Tests.csproj

# Discover available tests (xUnit v3 + Microsoft.Testing.Platform)
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

# Common filter variants
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

### Taskfile Commands (if task is installed)
```bash
task format      # Code formatting with CSharpier
task clean       # Clean build artifacts
task restore     # Restore NuGet dependencies
task build       # Build Release configuration
task pack        # Create NuGet packages
task pack-local  # Publish to local NuGet source
```

## Code Style Guidelines

### C# Code Style
- **Indentation**: 4 spaces (enforced by .editorconfig)
- **Line Length**: 100 characters maximum
- **File-scoped namespaces**: Preferred over block-scoped
- **Nullable Reference Types**: Always enabled
- **Target-typed new**: Use `new()` when type is obvious
- **Line endings**: LF for C# files
- **Trailing whitespace**: Always trimmed

### Naming Conventions/
- **Classes/Records**: PascalCase with descriptive suffixes (`MapperClassInfo`, `PropertyAnalysis`)
- **Methods**: PascalCase for public, camelCase for private
- **Constants**: PascalCase with underscores (`MapperSyntaxProvider_Extract`)
- **Private Fields**: camelCase with underscore prefix when needed (`_lazyWellKnownTypes`)
- **Test Methods**: Descriptive names with underscores for readability (`Simple_HelloWorld`)

### Import Organization
```csharp
// External libraries first
using Microsoft.CodeAnalysis;
using Amazon.DynamoDBv2.Model;

// Internal project namespaces (file-scoped)
namespace DynamoMapper.Generator;
```

### Error Handling Patterns
```csharp
// Use DiagnosticResult<T> for functional error handling
return DiagnosticResult<PropertyAnalysis>.Success(analysis);

// Or failure with diagnostics
return DiagnosticResult<PropertyAnalysis>.Failure(
    DiagnosticDescriptors.CannotConvertFromAttributeValue,
    locationInfo,
    propertyName,
    typeName
);

// Use Map/Bind for chaining operations
return result.Map(value => Transform(value)).Bind(transformed => Validate(transformed));
```

### Culture and Localization
- **Always use InvariantCulture** for numeric conversions:
```csharp
int.Parse(stringValue, NumberStyles.Integer, CultureInfo.InvariantCulture);
value.ToString(CultureInfo.InvariantCulture);
```

### Documentation Requirements
- **XML documentation** for all public APIs
- Use `<see cref=""/>` for cross-references
- Include `<remarks>` for implementation details
- Comprehensive documentation with examples

### Testing Patterns
- Use raw string literals for test code
- FluentAssertions for readable assertions
- Custom assertion messages with context
- Snapshot testing with Verify.SourceGenerators

### Performance Guidelines
- Zero-allocation patterns in hot paths
- Use arrays instead of LINQ when possible
- Cache expensive operations with BoundedCacheWithFactory
- Early returns for cancellation and invalid states

### Code Generation Specific
- Use tracking names for incremental generation
- Deterministic output required
- Generated code must include auto-generated header
- Follow existing Scriban template patterns

### Key Principles
1. **Domain models stay clean** - No attributes on POCOs
2. **Convention-first** - Minimal configuration required
3. **Type-safe** - Compile-time validation
4. **High-performance** - Zero reflection, minimal allocations
5. **Single-table focused** - Designed for DynamoDB patterns
6. **Functional error handling** - Use Result<T> patterns
7. **Culture-safe** - Always use InvariantCulture
8. **Deterministic generation** - Consistent output for builds