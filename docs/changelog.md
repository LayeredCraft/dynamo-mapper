# Changelog

All notable changes to DynamoMapper will be documented here.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Nested object and nested collection mapping in the source generator.
- Mapper registry lookup with inline fallback for nested types.
- Diagnostics for nested cycles and invalid dot-notation paths (DM0006-DM0008).
- `examples/DynamoMapper.Nested` for end-to-end nested mapping scenarios.

### Planned
- Phase 1: Attribute-based mapping
- Phase 2: Fluent DSL configuration

See [Roadmap](roadmap/phase-1.md) for details.
