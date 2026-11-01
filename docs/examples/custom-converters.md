# Custom converters

See [Static conversion methods](../usage/static-converters.md) for a complete mapper
that writes a decimal as a DynamoDB string using invariant culture. Custom write
methods receive the whole source model; custom read methods receive the whole item
dictionary. Converter interfaces are not implemented.
