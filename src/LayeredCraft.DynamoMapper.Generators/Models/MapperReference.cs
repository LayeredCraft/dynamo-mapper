namespace DynamoMapper.Generator.Models;

/// <summary>
///     Reference to a mapper class that can map a specific model type.
/// </summary>
/// <param name="MapperFullyQualifiedName">
///     The fully qualified name of the mapper class (e.g., "global::MyNamespace.AddressMapper").
/// </param>
/// <param name="HasToItemMethod">Whether the mapper has a ToItem method for serialization.</param>
/// <param name="HasFromItemMethod">Whether the mapper has a FromItem method for deserialization.</param>
internal sealed record MapperReference(
    string MapperFullyQualifiedName,
    bool HasToItemMethod,
    bool HasFromItemMethod
);
