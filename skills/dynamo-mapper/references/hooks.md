# Hooks

## Supported lifecycle hooks

Hooks are optional `static partial void` methods on the mapper class.

- `BeforeToItem(T source, Dictionary<string, AttributeValue> item)`
- `AfterToItem(T source, Dictionary<string, AttributeValue> item)`
- `BeforeFromItem(Dictionary<string, AttributeValue> item)`
- `AfterFromItem(Dictionary<string, AttributeValue> item, ref T entity)`

Where `T` is the mapper model type.

## Generation behavior

- hooks are discovered by exact method names above
- hooks can be declared and implemented in separate parts of the same partial mapper class
- one-way mappers are supported; only hooks for generated directions are emitted
- no `To*` hooks keeps `To*` expression-bodied
- any `To*` hook switches `To*` generation to block body

## Execution order

- `To*`: create dictionary -> `BeforeToItem` -> map members -> `AfterToItem` -> return
- `From*`: `BeforeFromItem` -> map/construct model -> `AfterFromItem` -> return

## Validation diagnostics

- `DM0401` invalid hook signature
  - wrong parameter count
  - wrong `ref` usage (`AfterFromItem` must use `ref` on entity)
  - non-void return type
  - non-partial hook method
- `DM0402` hook is not static
- `DM0403` hook parameter types do not match mapper model/dictionary requirements

These diagnostics are warnings.

## Safe guidance

- use hooks for DynamoDB-specific concerns (PK/SK composition, TTL, metadata, normalization)
- keep hook logic focused; avoid placing domain business workflows in hooks
