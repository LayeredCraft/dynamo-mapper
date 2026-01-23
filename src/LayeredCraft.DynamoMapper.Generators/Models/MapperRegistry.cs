using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace DynamoMapper.Generator.Models;

/// <summary>
///     Registry of all discovered mapper classes, keyed by the fully qualified name of the model type they map.
/// </summary>
/// <param name="ModelToMapper">
///     Dictionary mapping model type fully qualified names to their corresponding mapper references.
/// </param>
internal sealed record MapperRegistry(
    ImmutableDictionary<string, MapperReference> ModelToMapper
)
{
    /// <summary>An empty mapper registry.</summary>
    internal static MapperRegistry Empty { get; } =
        new(ImmutableDictionary<string, MapperReference>.Empty);

    /// <summary>
    ///     Tries to get the mapper reference for a given model type.
    /// </summary>
    /// <param name="modelTypeSymbol">The model type symbol to look up.</param>
    /// <param name="mapperReference">The mapper reference if found.</param>
    /// <returns>True if a mapper was found for the model type.</returns>
    internal bool TryGetMapper(ITypeSymbol modelTypeSymbol, out MapperReference? mapperReference)
    {
        var fullyQualifiedName = modelTypeSymbol.ToDisplayString(
            SymbolDisplayFormat.FullyQualifiedFormat
        );
        return ModelToMapper.TryGetValue(fullyQualifiedName, out mapperReference);
    }

    /// <summary>
    ///     Checks if a mapper exists for the given model type.
    /// </summary>
    /// <param name="modelTypeSymbol">The model type symbol to check.</param>
    /// <returns>True if a mapper exists for the model type.</returns>
    internal bool HasMapper(ITypeSymbol modelTypeSymbol)
    {
        var fullyQualifiedName = modelTypeSymbol.ToDisplayString(
            SymbolDisplayFormat.FullyQualifiedFormat
        );
        return ModelToMapper.ContainsKey(fullyQualifiedName);
    }
}
