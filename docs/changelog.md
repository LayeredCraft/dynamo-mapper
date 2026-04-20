# Changelog

All notable changes to LayeredCraft.DynamoMapper will be documented here.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Breaking Changes

- **Package renamed**: NuGet package renamed from `DynamoMapper` to `LayeredCraft.DynamoMapper`. This package supersedes `DynamoMapper`.
- **Namespace renamed**: All public namespaces changed from `DynamoMapper.Runtime` to `LayeredCraft.DynamoMapper.Runtime`.
  - Update all `using DynamoMapper.Runtime;` to `using LayeredCraft.DynamoMapper.Runtime;`.
- **Assembly renamed**: Runtime assembly is now `LayeredCraft.DynamoMapper.Runtime.dll`.

### Added
- Nested object and nested collection mapping in the source generator.
- Mapper registry lookup with inline fallback for nested types.
- Diagnostics for nested cycles and invalid dot-notation paths (DM0006-DM0008).
- `examples/DynamoMapper.Nested` for end-to-end nested mapping scenarios.

### Fixed

- Nested `FromItem` fallback behavior now honors effective requiredness for nested object and
  nested collection containers.
- Non-nullable optional nested object fallbacks now emit `null!` to avoid CS8601 warnings while
  preserving optional semantics.

### Planned
- Phase 1: Attribute-based mapping
- Phase 2: Fluent DSL configuration

See [Roadmap](roadmap/phase-1.md) for details.
