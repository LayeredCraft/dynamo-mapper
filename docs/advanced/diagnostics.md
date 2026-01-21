# Diagnostics

This page lists the diagnostics emitted by the generator and when they occur.

## Usage Diagnostics

| Id | Title | Trigger |
| --- | --- | --- |
| DM0001 | Type cannot be mapped to an AttributeValue | A property type is not supported for mapping. |
| DM0003 | Collection element type not supported | A collection element type is unsupported (non-primitive, non-nested). |
| DM0004 | Dictionary key must be string | A map property uses a non-string key type. |
| DM0005 | Incompatible DynamoKind override for collection | `DynamoKind` override does not match the inferred collection kind. |
| DM0006 | Circular reference detected in nested type | Nested object graphs contain a cycle (direct or indirect). |
| DM0007 | Unsupported nested member type | A nested property type cannot be mapped. |
| DM0008 | Invalid dot-notation path | A dot-notation override points to a missing property. |
| DM0101 | No mapper methods found | A `[DynamoMapper]` class has no `To*` or `From*` partial methods. |
| DM0102 | Mapper methods use different POCO types | `ToItem`/`FromItem` use different model types. |
| DM0103 | Multiple constructors marked with `[DynamoMapperConstructor]` | More than one constructor is attributed. |

## Notes

- Nested mapping cycles are detected during analysis and reported as `DM0006`.
- Dot-notation overrides are validated against the model graph and reported as `DM0008`.
