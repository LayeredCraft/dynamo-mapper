# Runtime Helpers

The runtime package exposes extension methods on
`Dictionary<string, AttributeValue>` for both generated code and manual use.

## Core Value Helpers

These methods live in `DynamoMapper.Runtime` and are used by the generator.

String:
- `GetString`, `GetNullableString`
- `SetString`

Boolean:
- `GetBool`, `GetNullableBool`
- `SetBool`

Numbers:
- `GetInt`, `GetNullableInt`
- `GetLong`, `GetNullableLong`
- `GetFloat`, `GetNullableFloat`
- `GetDouble`, `GetNullableDouble`
- `GetDecimal`, `GetNullableDecimal`
- `GetShort`, `GetNullableShort`
- `GetByte`, `GetNullableByte`
- `SetInt`, `SetLong`, `SetFloat`, `SetDouble`, `SetDecimal`, `SetShort`, `SetByte`

Date/Time:
- `GetDateTime`, `GetNullableDateTime`
- `GetDateTimeOffset`, `GetNullableDateTimeOffset`
- `GetTimeSpan`, `GetNullableTimeSpan`
- `SetDateTime`, `SetDateTimeOffset`, `SetTimeSpan`

Guid:
- `GetGuid`, `GetNullableGuid`
- `SetGuid`

Enum:
- `GetEnum<T>`, `GetNullableEnum<T>`
- `SetEnum<T>`

## Collection Helpers

Lists and maps:
- `GetList<T>`, `GetNullableList<T>`
- `SetList<T>`
- `GetMap<T>`, `GetNullableMap<T>`
- `SetMap<T>`

Sets:
- `GetStringSet`, `GetNullableStringSet`, `SetStringSet`
- `GetNumberSet<T>`, `GetNullableNumberSet<T>`, `SetNumberSet<T>`
- `GetBinarySet`, `GetNullableBinarySet`, `SetBinarySet`

## Parameters

- `Requiredness` controls missing-attribute behavior (`Required`, `Optional`,
  `InferFromNullability`).
- `DynamoKind` lets you override the inferred DynamoDB kind.

## Example

```csharp
var attributes = new Dictionary<string, AttributeValue>();

attributes
    .SetString("pk", "USER#123")
    .SetInt("version", 2)
    .SetList("tags", new List<string> { "alpha", "beta" })
    .SetMap("metadata", new Dictionary<string, int> { ["count"] = 42 })
    .SetBinarySet("payloads", new List<byte[]> { new byte[] { 1, 2, 3 } });

var tags = attributes.GetList<string>("tags");
var version = attributes.GetInt("version");
```
