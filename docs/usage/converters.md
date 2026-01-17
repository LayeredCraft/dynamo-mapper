---
title: Converter Types
description: Converter types are not supported in the current release
---

# Converter Types

Converter types (`IDynamoConverter<T>`) are **not supported** in the current release.

If you need custom conversions today, use **static conversion methods** on the mapper
class via `[DynamoField(ToMethod = ..., FromMethod = ...)]`.

See:
- [Static Conversion Methods](static-converters.md)
- [Attributes](../api-reference/attributes.md)
